namespace NetNotepad.Base
{
    public static class EndpointsContract
    {
        private static HookBuilder _hb = new("/");

        private static HookBuilder _auth = _hb.Add("auth");
        public static string Login => _auth.Build("/login");
        public static string Register => _auth.Build("/register");
        public static string Remove => _auth.Build("/remove");
        public static string Logout => _auth.Build("/logout");
        public static string User => _auth.Build("/user");

        private static HookBuilder _user = _hb.Add("user");


        private static HookBuilder _notepad = _hb.Add("notepad");

    }
}