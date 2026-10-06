using Newtonsoft.Json;

namespace NetNotepad.Client.Source.Localization
{
    public class LocalizationPair<T>(string key, T TValue) : LocalizationPair(key, TValue ?? new object())
    {
        public override string Key { get; set; } = key;
        [JsonIgnore] public override object Value { get; set; } = TValue ?? new object();
        public T TValue => (T)Value;
    }

    public class LocalizationPair(string key, object value)
    {
        public virtual string Key { get; set; } = key;
        public virtual object Value { get; set; } = value;
    }
}
