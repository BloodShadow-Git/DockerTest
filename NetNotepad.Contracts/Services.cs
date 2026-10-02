using System.Net;

namespace NetNotepad.Contracts
{
    public record Responce(HttpStatusCode HttpCode, string Message);
    public record ServiceRequest(string Path, string Method, string Payload);
    public record ServiceResponce(HttpStatusCode HttpCode, string Message) : Responce(HttpCode, Message);
}