using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetNotepad.ServiceBase
{
    public static class SerializeModule
    {
        private static readonly JsonSerializerSettings _settings;

        static SerializeModule()
        {
            _settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
        }

        public static byte[] Serialize(object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
        public static Task<byte[]> SerializeAsync(object obj) => Task.Run(() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings)));
        public static T Deserialize<T>(byte[] source) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(source), _settings)!;
        public static Task<T> DeserializeAsync<T>(byte[] source) => Task.Run(() => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(source), _settings)!);
        public static object TryDeserialize(byte[] source, params Type[] types)
        {
            if (types.Length <= 0) { return null; }
            try
            {
                string sourceSTR = Encoding.UTF8.GetString(source);
                var jobject = JObject.Parse(sourceSTR);

                Type? first = null;
                int maxMatches = -1;

                foreach (var type in types)
                {
                    var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    int curr = 0;

                    foreach (var prop in props) { if (jobject.ContainsKey(prop.Name)) { curr++; } }
                    if (curr > maxMatches && curr > 0)
                    {
                        maxMatches = curr;
                        first = type;
                    }
                }
                if (first != null) { return jobject.ToObject(first, JsonSerializer.Create(_settings)); }
            }
            catch { }
            return null;
        }
    }
}
