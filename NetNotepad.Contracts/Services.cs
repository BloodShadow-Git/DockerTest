namespace NetNotepad.Contracts
{
    public record Responce(string HttpCode);
    public record ServiceRequest(string Path, string Payload);
    public record ServiceResponce(string HttpCode, string Message) : Responce(HttpCode);
}