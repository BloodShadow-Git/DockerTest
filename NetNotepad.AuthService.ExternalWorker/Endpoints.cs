using System.Net;
using System.Security.Cryptography;
using NetNotepad.Contracts;
using NetNotepad.ServiceBase;
using RabbitMQ.AMQP.Client.Impl;

namespace NetNotepad.AuthService
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
            LoginRequest lr = SerializeModule.Deserialize<LoginRequest>(payload);
            if (lr == null) { return new ServiceResponce(HttpStatusCode.BadRequest, "BAD_JSON_BODY"); }
            using AppDBContext db = new();
            User? user = db.Users.Where(d => d.UserName == lr.Login).FirstOrDefault();
            if (user == null) { return new ServiceResponce(HttpStatusCode.Unauthorized, "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            if (!BCrypt.Net.BCrypt.Verify(lr.Password, user.PasswordHash)) { return new ServiceResponce(HttpStatusCode.Unauthorized, "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            Program.EventHandler.Publish(Events.USER_LOGIN, new UserLogin(user.UserGuid));
            return GenerateTokens(user, db, lr);
        }
        private static Responce Register(byte[] payload)
        {
            LoginRequest lr = SerializeModule.Deserialize<LoginRequest>(payload);
            if (lr == null) { return new ServiceResponce(HttpStatusCode.BadRequest, "BAD_JSON_BODY"); }
            using AppDBContext db = new();
            if (db.Users.Where(d => d.UserName == lr.Login).Any()) { return new ServiceResponce(HttpStatusCode.Conflict, "ACCOUNT_ALREADY_REGISTERED"); }
            else
            {
                User user = new(Guid.NewGuid(), lr.Login, BCrypt.Net.BCrypt.HashPassword(lr.Password));
                db.Users.Add(user);
                db.SaveChanges();
                Program.EventHandler.Publish(Events.USER_CREATED, new UserCreated(user.UserGuid, user.UserName));
                return GenerateTokens(user, db, lr);
            }
        }

        private static Responce RemoveUser(byte[] payload)
        {

            return new Responce(HttpStatusCode.OK, "USER_REMOVED");
        }

        private static LoginResponce GenerateTokens(User user, AppDBContext db, LoginRequest lr)
        {
            string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            TimeSpan ttl = lr.Persistent ? TimeSpan.FromDays(30) : TimeSpan.FromDays(1);
            db.RefreshTokens.Add(new(user.UserGuid, refreshToken, DateTime.UtcNow, ttl, lr.DeviceName));
            db.SaveChanges();
            return new(HttpStatusCode.OK, "CREATED",
                    Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                    refreshToken, ttl);
        }
    }
}