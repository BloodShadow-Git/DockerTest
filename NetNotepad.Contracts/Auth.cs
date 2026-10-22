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
    public record LoginResponse(HttpStatusCode HttpCode, string Message, string JWT, string RefreshToken, Guid DeviceGuid, DateTime CreationDate) : Response(HttpCode, Message);
    public record RegisterRequest(string Login, string Password, bool Persistent, string DeviceName);
    public record RegisterResponse(HttpStatusCode HttpCode, string Message, string JWT, string RefreshToken, Guid DeviceGuid, DateTime CreationDate) : Response(HttpCode, Message);
    public record RemoveRequest(string JWT);
    public record LogoutRequest(string JWT);
    public record LogoutRTRequest(string JWT, params Guid[] DeviceGUIDs);
    public record UpdatePassRequest(string JWT, string OldPassword, string NewPassword);
    public record UpdateRTRequest(string JWT, string Password, TimeSpan NewRefreshTokenTTL);
    public record UserGetRequest(string JWT);
    public record UserGetResponse(HttpStatusCode HttpCode, string Message, Guid UserGuid, string UserLogin, TimeSpan RefreshTokenTTL) : Response(HttpCode, Message);
}