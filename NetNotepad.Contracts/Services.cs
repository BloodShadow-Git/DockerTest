using System.Net;

namespace NetNotepad.Contracts
{
    public record Responce(HttpStatusCode HttpCode, string Message);
    public record ServiceRequest(string Path, string Method, string Payload);
    public record ServiceResponce(HttpStatusCode HttpCode, string Message) : Responce(HttpCode, Message);

    public record ServiceExceptions(HttpStatusCode Code, string Message)
    {
        public static readonly ServiceExceptions INVALID_TOKEN = new(HttpStatusCode.Unauthorized, nameof(INVALID_TOKEN));
        public static readonly ServiceExceptions BAD_JSON_BODY = new(HttpStatusCode.BadRequest, nameof(BAD_JSON_BODY));
        public static readonly ServiceExceptions LOGIN_OR_PASSWORD_IS_INCORRECT = new(HttpStatusCode.Unauthorized, nameof(LOGIN_OR_PASSWORD_IS_INCORRECT));
        public static readonly ServiceExceptions ACCOUNT_ALREADY_REGISTERED = new(HttpStatusCode.Conflict, nameof(ACCOUNT_ALREADY_REGISTERED));
        public static readonly ServiceExceptions INVALID_JWT = new(HttpStatusCode.Unauthorized, nameof(INVALID_JWT));
        public static readonly ServiceExceptions USER_NOT_EXISTS = new(HttpStatusCode.NotFound, nameof(USER_NOT_EXISTS));
        public static readonly ServiceExceptions NOT_ALL_TOKENS_ARE_VALID = new(HttpStatusCode.BadRequest, nameof(NOT_ALL_TOKENS_ARE_VALID));

        public static implicit operator Responce(ServiceExceptions ex) => new(ex.Code, ex.Message);
        public static implicit operator ServiceExceptions(Responce resp) => new(resp.HttpCode, resp.Message);
    }
}