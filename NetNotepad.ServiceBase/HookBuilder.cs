namespace NetNotepad.ServiceBase
{
    public class HookBuilder(string parent)
    {
        private string? _parent = parent;
        public string Build(string path) => _parent + path;
    }

    public class HookRouter<T>
    {
        public IReadOnlyDictionary<string, Func<byte[], T>> HooksMap => _hooksMap;
        private Dictionary<string, Func<byte[], T>> _hooksMap = [];

        public bool Route(string destination, byte[] args, out T? result)
        {
            result = default;
            if (!_hooksMap.TryGetValue(destination, out Func<byte[], T>? callback)) { return false; }
            result = callback.Invoke(args);
            return true;
        }
        public bool Add(string route, Func<byte[], T> callback) => _hooksMap.TryAdd(route, callback);
        public bool Remove(string route) => _hooksMap.Remove(route);
    }
}