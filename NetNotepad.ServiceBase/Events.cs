namespace NetNotepad.ServiceBase
{
    public static class Events
    {
        public const string USER_CREATED = "user.created";
        public const string USER_LOGIN = "user.login";
        public const string USER_REMOVED = "user.removed";
        public const string USER_LOGOUT = "user.logout";
    }

    public record UserCreated(Guid UserGuid, string UserLogin);
    public record UserLogin(Guid UserGuid);
    public record UserRemove(Guid UserGuid);
    public record UserLogout(Guid UserGuid);
}