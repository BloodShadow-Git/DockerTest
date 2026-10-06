using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NetNotepad.Base;
using NetNotepad.Contracts;
using NetNotepad.ServiceBase;

namespace NetNotepad.AuthService.ExternalWorker
{
    public static class Endpoints
    {
        public static void AddEndpoints()
        {
            Program.HTTPHookRouter.Add(EndpointsContract.Login, Login);
            Program.HTTPHookRouter.Add(EndpointsContract.Register, Register);
            Program.HTTPHookRouter.Add(EndpointsContract.Remove, RemoveUser);
            Program.HTTPHookRouter.Add(EndpointsContract.Logout, Logout);
            Program.HTTPHookRouter.Add(EndpointsContract.User, Update, HttpMethod.Post);
            Program.HTTPHookRouter.Add(EndpointsContract.User, User, HttpMethod.Get);
        }

        private static Responce Login(byte[] payload)
        {
            return SerializeModule.TryDeserialize(payload, typeof(LoginRequest), typeof(LoginRTRequest)) switch
            {
                LoginRequest lr => LoginLR(lr),
                LoginRTRequest lrtr => LoginLRTR(lrtr),
                _ => ServiceExceptions.BAD_JSON_BODY,
            };
        }
        private static Responce LoginLR(LoginRequest lr)
        {
            using AppDBContext db = new();
            User? user = db.Users.AsNoTracking().FirstOrDefault(d => d.UserLogin == lr.Login);
            if (user == null) { return ServiceExceptions.LOGIN_OR_PASSWORD_IS_INCORRECT; }
            if (!BCrypt.Net.BCrypt.Verify(lr.Password, user.PasswordHash)) { return ServiceExceptions.LOGIN_OR_PASSWORD_IS_INCORRECT; }
            RefreshTokenData rtd = GenerateTokens(user, lr.Persistent, lr.DeviceName);
            db.RefreshTokens.Add(rtd);
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_LOGIN, new UserLogin(user.UserGuid));
            return new LoginResponce(HttpStatusCode.OK, "CREATED",
                Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                rtd.RefreshToken, rtd.DeviceGuid, rtd.ExpireDate);
        }
        private static Responce LoginLRTR(LoginRTRequest lrtr)
        {
            using AppDBContext db = new();
            RefreshTokenData? rtd = db.RefreshTokens.AsNoTracking().FirstOrDefault(d => d.RefreshToken == lrtr.RefreshToken);
            if (rtd == null || rtd.ExpireDate <= DateTime.UtcNow)
            {
                if (rtd != null)
                {
                    db.Remove(rtd);
                    db.SaveChanges();
                }
                return ServiceExceptions.INVALID_TOKEN;
            }
            User user = db.Users.AsNoTracking().First(d => d.UserGuid == rtd.UserGuid);
            rtd = GenerateTokens(user, rtd.Persistent, lrtr.DeviceName, rtd.DeviceGuid);
            db.RefreshTokens.Update(rtd);
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_LOGIN, new UserLogin(user.UserGuid));
            return new LoginResponce(HttpStatusCode.OK, "CREATED",
                Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                rtd.RefreshToken, rtd.DeviceGuid, rtd.ExpireDate);
        }

        private static Responce Register(byte[] payload)
        {
            RegisterRequest rr = SerializeModule.Deserialize<RegisterRequest>(payload);
            if (rr == null) { return ServiceExceptions.BAD_JSON_BODY; }
            using AppDBContext db = new();
            if (db.Users.AsNoTracking().Any(d => d.UserLogin == rr.Login)) { return ServiceExceptions.ACCOUNT_ALREADY_REGISTERED; }
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
                    rtd.RefreshToken, rtd.DeviceGuid, rtd.ExpireDate);
            }
        }

        private static RefreshTokenData GenerateTokens(User user, bool persistent, string deviceName, Guid? tokenGuid = null)
        {
            string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            TimeSpan ttl = persistent ? user.RefreshTokenTTL : TimeSpan.FromHours(12);
            return new(user.UserGuid, tokenGuid ?? Guid.NewGuid(), refreshToken, DateTime.UtcNow + ttl, deviceName, persistent);
        }

        private static Responce RemoveUser(byte[] payload)
        {
            RemoveRequest rr = SerializeModule.Deserialize<RemoveRequest>(payload);
            if (rr == null) { return ServiceExceptions.BAD_JSON_BODY; }
            if (!Program.JWTValidator.Validate(rr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            using AppDBContext db = new();
            User? user = db.Users.AsNoTracking().FirstOrDefault(d => d.UserGuid == userGuid);
            if (user == null) { return ServiceExceptions.USER_NOT_EXISTS; }
            db.Users.Remove(user);
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_REMOVED, new UserRemove(userGuid));
            return new Responce(HttpStatusCode.OK, "USER_REMOVED");
        }

        private static Responce Logout(byte[] payload)
        {
            return SerializeModule.TryDeserialize(payload, typeof(LogoutRequest), typeof(LogoutRTRequest)) switch
            {
                LogoutRequest lr => LogoutLR(lr),
                LogoutRTRequest lrtr => LogoutLRTR(lrtr),
                _ => ServiceExceptions.BAD_JSON_BODY,
            };
        }
        private static Responce LogoutLR(LogoutRequest lr)
        {
            if (!Program.JWTValidator.Validate(lr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            using AppDBContext db = new();
            db.RefreshTokens.Where(u => u.UserGuid == userGuid).ExecuteDelete();
            db.SaveChanges();
            Program.EventHandler.Publish(Events.USER_LOGOUT, new UserLogout(userGuid));
            return new Responce(HttpStatusCode.OK, "USER_LOGOUT");
        }
        private static Responce LogoutLRTR(LogoutRTRequest lrtr)
        {
            if (!Program.JWTValidator.Validate(lrtr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            AppDBContext db = new();
            Guid[] tokens = [.. lrtr.RefreshTokens.Distinct()];
            RefreshTokenData[] rtds = [.. db.RefreshTokens.Where(d => tokens.Contains(d.DeviceGuid))];
            if (rtds.Any(d => d.UserGuid != userGuid) ||
                rtds.Length != tokens.Length) { return ServiceExceptions.NOT_ALL_TOKENS_ARE_VALID; }
            db.RefreshTokens.RemoveRange(rtds);
            db.SaveChanges();
            return new Responce(HttpStatusCode.OK, "USER_LOGOUT");
        }

        private static Responce Update(byte[] payload)
        {
            return SerializeModule.TryDeserialize(payload, typeof(UpdatePassRequest), typeof(UpdateRTRequest)) switch
            {
                UpdatePassRequest upr => UpdatePass(upr),
                UpdateRTRequest urtr => UpdateRT(urtr),
                _ => ServiceExceptions.BAD_JSON_BODY,
            };
        }

        private static Responce UpdatePass(UpdatePassRequest upr)
        {
            if (!Program.JWTValidator.Validate(upr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            AppDBContext db = new();
            User user = db.Users.AsNoTracking().First(d => d.UserGuid == userGuid);
            if (BCrypt.Net.BCrypt.Verify(upr.OldPassword, user.PasswordHash)) { return ServiceExceptions.LOGIN_OR_PASSWORD_IS_INCORRECT; }
            user = new(user.UserGuid, user.UserLogin, BCrypt.Net.BCrypt.HashPassword(upr.NewPassword), user.RefreshTokenTTL);
            db.Users.Update(user);
            db.RefreshTokens.Where(u => u.UserGuid == userGuid).ExecuteDelete();
            db.SaveChanges();
            return new Responce(HttpStatusCode.OK, "PASSWORD_UPDATED");
        }

        private static Responce UpdateRT(UpdateRTRequest urtr)
        {
            if (!Program.JWTValidator.Validate(urtr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            AppDBContext db = new();
            User user = db.Users.AsNoTracking().First(d => d.UserGuid == userGuid);
            if (BCrypt.Net.BCrypt.Verify(urtr.Password, user.PasswordHash)) { return ServiceExceptions.LOGIN_OR_PASSWORD_IS_INCORRECT; }
            user = new(user.UserGuid, user.UserLogin, user.PasswordHash, urtr.NewRefreshTokenTTL);
            db.Users.Update(user);
            db.SaveChanges();
            return new Responce(HttpStatusCode.OK, "TTL_UPDATED");
        }

        private static Responce User(byte[] payload)
        {
            UserGetRequest ugr = SerializeModule.Deserialize<UserGetRequest>(payload);
            if (ugr == null) { return ServiceExceptions.BAD_JSON_BODY; }
            if (!Program.JWTValidator.Validate(ugr.JWT, out ClaimsPrincipal principal)) { return ServiceExceptions.INVALID_JWT; }
            Guid userGuid = Guid.Parse(principal.FindFirst(JWTNames.SUB)!.Value);
            using AppDBContext db = new();
            User user = db.Users.AsNoTracking().First(d => d.UserGuid == userGuid);
            return new UserGetResponce(HttpStatusCode.OK, "USER_GETTED", user.UserGuid, user.UserLogin, user.RefreshTokenTTL);
        }
    }
}