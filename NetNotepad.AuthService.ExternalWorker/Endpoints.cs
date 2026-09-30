using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NetNotepad.Contracts;
using NetNotepad.ServiceBase;

namespace NetNotepad.AuthService.ExternalWorker
{
    public static class Endpoints
    {
        public static void AddEndpoints()
        {
            HookBuilder hb = new("/auth");
            Program.HTTPHookRouter.Add(hb.Build("/login"), Login);
            Program.HTTPHookRouter.Add(hb.Build("/register"), Register);
            Program.HTTPHookRouter.Add(hb.Build("/remove"), RemoveUser);
        }

        private static Responce Login(byte[] payload)
        {
            return SerializeModule.TryDeserialize(payload, typeof(LoginRequest), typeof(LoginRTRequest)) switch
            {
                LoginRequest lr => LoginLR(lr),
                LoginRTRequest lrtr => LoginLRTR(lrtr),
                _ => new ServiceResponce(HttpStatusCode.BadRequest, "BAD_JSON_BODY"),
            };
        }
        private static Responce LoginLR(LoginRequest lr)
        {
            using AppDBContext db = new();
            User? user = db.Users.AsNoTracking().FirstOrDefault(d => d.UserLogin == lr.Login);
            if (user == null) { return new ServiceResponce(HttpStatusCode.Unauthorized, "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            if (!BCrypt.Net.BCrypt.Verify(lr.Password, user.PasswordHash)) { return new ServiceResponce(HttpStatusCode.Unauthorized, "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            RefreshTokenData rtd = GenerateTokens(user, lr.Persistent, lr.DeviceName);
            db.RefreshTokens.Add(rtd);
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_LOGIN, new UserLogin(user.UserGuid));
            return new RegisterResponce(HttpStatusCode.OK, "CREATED",
                Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                rtd.RefreshToken, rtd.ExpireDate);
        }
        private static Responce LoginLRTR(LoginRTRequest lrtr)
        {
            using AppDBContext db = new();
            RefreshTokenData? rtd = db.RefreshTokens.FirstOrDefault(d => d.RefreshToken == lrtr.RefreshToken);
            if (rtd == null) { return new ServiceResponce(HttpStatusCode.Unauthorized, "INVALID_TOKEN"); }
            User user = db.Users.AsNoTracking().First(d => d.UserGuid == rtd.UserGuid);
            db.RefreshTokens.Remove(rtd);
            rtd = GenerateTokens(user, rtd.Persistent, lrtr.DeviceName);
            db.RefreshTokens.Add(rtd);
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_LOGIN, new UserLogin(user.UserGuid));
            return new LoginResponce(HttpStatusCode.OK, "CREATED",
                Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                rtd.RefreshToken, rtd.ExpireDate);
        }

        private static Responce Register(byte[] payload)
        {
            RegisterRequest rr = SerializeModule.Deserialize<RegisterRequest>(payload);
            if (rr == null) { return new ServiceResponce(HttpStatusCode.BadRequest, "BAD_JSON_BODY"); }
            using AppDBContext db = new();
            if (db.Users.AsNoTracking().Any(d => d.UserLogin == rr.Login)) { return new ServiceResponce(HttpStatusCode.Conflict, "ACCOUNT_ALREADY_REGISTERED"); }
            else
            {
                User user = new(Guid.NewGuid(), rr.Login, BCrypt.Net.BCrypt.HashPassword(rr.Password), TimeSpan.FromDays(30));
                RefreshTokenData rtd = GenerateTokens(user, rr.Persistent, rr.DeviceName);
                db.Users.Add(user);
                db.RefreshTokens.Add(rtd);
                db.SaveChanges();
                Program.EventHandler.Publish(Events.USER_CREATED, new UserCreated(user.UserGuid, user.UserLogin));
                return new RegisterResponce(HttpStatusCode.OK, "CREATED",
                    Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                    rtd.RefreshToken, rtd.ExpireDate);
            }
        }

        private static Responce RemoveUser(byte[] payload)
        {
            RemoveRequest rr = SerializeModule.Deserialize<RemoveRequest>(payload);
            if (rr == null) { return new ServiceResponce(HttpStatusCode.BadRequest, "BAD_JSON_BODY"); }
            if (!Program.JWTValidator.Validate(rr.JWT, out ClaimsPrincipal principal)) { return new ServiceResponce(HttpStatusCode.Unauthorized, "INVALID_JWT"); }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            using AppDBContext db = new();
            User? user = db.Users.AsNoTracking().FirstOrDefault(d => d.UserGuid == userGuid);
            if (user == null) { return new ServiceResponce(HttpStatusCode.NotFound, "USER_NOT_EXISTS"); }
            db.Users.Remove(user);
            db.SaveChanges();
            return new Responce(HttpStatusCode.OK, "USER_REMOVED");
        }

        private static RefreshTokenData GenerateTokens(User user, bool persistent, string deviceName)
        {
            string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            TimeSpan ttl = persistent ? TimeSpan.FromDays(30) : TimeSpan.FromHours(12);
            return new(user.UserGuid, refreshToken, DateTime.UtcNow + ttl, deviceName, persistent);
        }
    }
}