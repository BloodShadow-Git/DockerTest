namespace NetNotepad.Contracts
{
    public static class JWTNames
    {
        public static readonly string SUB = "UserGUID";
        public static readonly string ISS = "AuthService";
        public static readonly string AUD = "Services";
    }
    public record LoginRequest(string Login, string Password);
    public record LoginResponce(string HttpCode, string JWT, string RefreshToken) : Responce(HttpCode);
}