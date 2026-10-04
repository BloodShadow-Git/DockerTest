using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.Source.Localization
{
    public static class LocalizationManager
    {
        public static IObservableCollection<string> AvailableLocalizations => _availableLocalizations;
        public static Observable<string> CurrentLocalization => _currentLocalization;
        public static IObservableCollection<(Type, string)> RegisteredKeys => _registeredKeys;

        private static readonly ObservableList<string> _availableLocalizations = [];
        private static readonly ReactiveProperty<string> _currentLocalization = new("ERROR");
        private static readonly ObservableHashSet<(Type, string)> _registeredKeys = [];
        private static readonly Dictionary<(string, string, Type), object> _localizations = [];

        public static bool RegisterKey<T>(string key)
        {
            Type type = typeof(T);
            bool result = _registeredKeys.Add((type, key));
            return result;
        }
        public static void SetLocalization(string localization) { if (_availableLocalizations.Contains(localization)) { _currentLocalization.Value = localization; } }
        public static void SetLocalization(int index) { if (index >= 0 && index < _availableLocalizations.Count) { SetLocalization(_availableLocalizations[index]); } }
        public static void AddLocalization(LocalizationData data)
        {
            if (!_availableLocalizations.Contains(data.LocalizationKey)) { _availableLocalizations.Add(data.LocalizationKey); }
            foreach (LocalizationPair pair in data.Pairs)
            {
                (string lang, string key, Type type) key = (data.LocalizationKey, pair.Key, pair.Value.GetType());
                _localizations[key] = pair.Value;
            }
        }
        public static void AddLocalization(params LocalizationData[] datas) { foreach (var data in datas) { AddLocalization(data); } }

        public static T Localize<T>(string key)
        {
            if (!_registeredKeys.Contains((typeof(T), key))) { throw new Exception("Key not registered"); }
            return LocalizeInternal<T>(key);
        }
        private static T LocalizeInternal<T>(string key)
        {
            if (_localizations.TryGetValue((_currentLocalization.CurrentValue, key, typeof(T)), out object? value)) { return (T)value; }
            else { return default!; }
        }
    }
}
