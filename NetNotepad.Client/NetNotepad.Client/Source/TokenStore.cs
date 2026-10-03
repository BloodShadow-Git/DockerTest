namespace NetNotepad.Client.Source
{
    public abstract class TokenStore
    {
        public static TokenStore Instance = null!;

        public static void Save(string path, object data) => Instance.SaveInternal(path, data);
        public static T? Load<T>(string path) => Instance.LoadInternal<T>(path);
        public static void Delete(string path) => Instance.DeleteInternal(path);
        public static bool Exists(string path) => Instance.ExistsInternal(path);

        protected abstract void SaveInternal(string path, object data);
        protected abstract T? LoadInternal<T>(string path);
        protected abstract void DeleteInternal(string path);
        protected abstract bool ExistsInternal(string path);
    }
}