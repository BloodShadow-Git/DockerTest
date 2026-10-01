using System.Net;

namespace NetNotepad.Contracts
{
    public static class JWTNames
    {
        public static readonly string SUB = "UserGUID";
        public static readonly string ISS = "AuthService";
        public static readonly string AUD = "Services";
    }
    public record LoginRequest(string Login, string Password, bool Persistent, string DeviceName);
    public record LoginRTRequest(string RefreshToken, string DeviceName);
    public record LoginResponce(HttpStatusCode HttpCode, string Message, string JWT, string RefreshToken, Guid DeviceGuid, DateTime ExpireDate) : Responce(HttpCode, Message);
    public record RegisterRequest(string Login, string Password, bool Persistent, string DeviceName);
    public record RegisterResponce(HttpStatusCode HttpCode, string Message, string JWT, string RefreshToken, Guid DeviceGuid, DateTime ExpireDate) : Responce(HttpCode, Message);
    public record RemoveRequest(string JWT);
    public record LogoutRequest(string JWT, params Guid[] RefreshTokens);
}