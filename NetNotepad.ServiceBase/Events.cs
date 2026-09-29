namespace NetNotepad.ServiceBase
{
    public static class Events
    {
        public const string USER_CREATED = "user.created";
        public const string USER_LOGIN = "user.login";
    }

    public record UserCreated(Guid UserGuid, string UserLogin);
    public record UserLogin(Guid UserGuid);
}