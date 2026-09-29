using NetNotepad.Contracts;

namespace NetNotepad.ServiceBase
{
    public class HookBuilder(string parent)
    {
        private string? _parent = parent;
        public string Build(string path) => _parent + path;
    }

    public class HookRouter
    {
        public IReadOnlyDictionary<string, Func<byte[], Responce>> HooksMap => _hooksMap;
        private Dictionary<string, Func<byte[], Responce>> _hooksMap = [];

        public bool Route(string destination, byte[] args, out Responce? result)
        {
            result = null;
            if (!_hooksMap.TryGetValue(destination, out Func<byte[], Responce>? callback)) { return false; }
            result = callback?.Invoke(args);
            return true;
        }
        public bool Add(string route, Func<byte[], Responce> callback) => _hooksMap.TryAdd(route, callback);
        public bool Remove(string route) => _hooksMap.Remove(route);
    }
}