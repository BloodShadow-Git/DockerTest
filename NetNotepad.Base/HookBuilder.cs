namespace NetNotepad.Base
{
    public class HookBuilder(string parent)
    {
        private string? _parent = parent;
        public string Build(string path) => _parent + path;
        public HookBuilder Add(string path) => new(_parent + path);
    }

    public class HookRouter<T>
    {
        public IReadOnlyDictionary<(string, HttpMethod?), Func<byte[], T>> HooksMap => _hooksMap;
        private Dictionary<(string, HttpMethod?), Func<byte[], T>> _hooksMap = [];

        public bool Route(string destination, HttpMethod method, byte[] args, out T? result)
        {
            result = default;
            if (!_hooksMap.TryGetValue((destination, method), out Func<byte[], T>? callback)) { if (!_hooksMap.TryGetValue((destination, null), out callback)) { return false; } }
            result = callback.Invoke(args);
            return true;
        }
        public bool Add(string route, Func<byte[], T> callback, HttpMethod? method = null) => _hooksMap.TryAdd((route, method), callback);
        public bool Remove(string route, HttpMethod? method = null) => _hooksMap.Remove((route, method));
    }
}