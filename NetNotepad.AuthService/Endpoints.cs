using System.Security.Cryptography;
using NetNotepad.Contracts;

namespace NetNotepad.AuthService
{
    public static class Endpoints
    {
        public static void AddEndpoints()
        {
            HookBuilder hb = new("/auth");
            Program.HTTPHookRouter.Add(hb.Build("/login"), Login);
            Program.HTTPHookRouter.Add(hb.Build("/register"), Register);
        }

        private static Responce Login(byte[] payload)
        {
            LoginRequest lr = SerializeModule.Deserialize<LoginRequest>(payload);
            if (lr == null) { return new ServiceResponce("400", "BAD_JSON_BODY"); }
            using AppDBContext db = new();
            User? user = db.Users.Where(d => d.UserName == lr.Login).FirstOrDefault();
            if (user == null) { return new ServiceResponce("401", "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            if (!BCrypt.Net.BCrypt.Verify(lr.Password, user.PasswordHash)) { return new ServiceResponce("401", "LOGIN_OR_PASSWORD_IS_INCORRECT"); }
            return GenerateTokens(user, db);
        }
        private static Responce Register(byte[] payload)
        {
            LoginRequest lr = SerializeModule.Deserialize<LoginRequest>(payload);
            if (lr == null) { return new ServiceResponce("400", "BAD_JSON_BODY"); }
            using AppDBContext db = new();
            if (db.Users.Where(d => d.UserName == lr.Login).Any()) { return new ServiceResponce("409", "ACCOUNT_ALREADY_REGISTERED"); }
            else
            {
                User user = new(Guid.NewGuid(), lr.Login, BCrypt.Net.BCrypt.HashPassword(lr.Password));
                db.Users.Add(user);
                db.SaveChanges();
                return GenerateTokens(user, db);
            }
        }

        private static LoginResponce GenerateTokens(User user, AppDBContext db)
        {
            string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            db.RefreshTokens.Add(new(user.UserGuid, refreshToken, DateTime.UtcNow, TimeSpan.FromDays(30)));
            db.SaveChanges();
            return new("200",
                    Program.JWTBuilder.Clone().SetLifeSpan(TimeSpan.FromMinutes(10)).SetUserGuid(user.UserGuid).Build(out _),
                    refreshToken);
        }
    }
}