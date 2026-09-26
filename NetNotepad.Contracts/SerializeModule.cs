using System.Text;
using Newtonsoft.Json;

namespace NetNotepad.Contracts
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
    }
}
