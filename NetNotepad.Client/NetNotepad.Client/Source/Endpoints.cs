using NetNotepad.Base;

namespace NetNotepad.Client.Source
{
    public static class Endpoints
    {
        private static HookBuilder _hb = new("/");

        private static HookBuilder _auth = _hb.Add("auth");
        public static string Login => _auth.Build("/login");
        public static string Register => _auth.Build("/register");
        public static string Remove => _auth.Build("/remove");
        public static string Logout => _auth.Build("/logout");

        private static HookBuilder _user = _hb.Add("user");


        private static HookBuilder _notepad = _hb.Add("notepad");

    }
}