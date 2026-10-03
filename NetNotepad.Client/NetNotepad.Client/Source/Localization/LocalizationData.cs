namespace NetNotepad.Client.Source.Localization
{
    public class LocalizationData(string key)
    {
        public string LocalizationKey { get; set; } = key;
        public LocalizationPair[] Pairs { get; set; } = [];
        public LocalizationData(string key, params LocalizationPair[] pairs) : this(key) { Pairs = pairs; }
    }
}
