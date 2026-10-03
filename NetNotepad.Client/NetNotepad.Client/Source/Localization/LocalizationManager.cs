using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.Source.Localization
{
    public class LocalizationManager
    {
        public IObservableCollection<string> AvailableLocalizations => _availableLocalizations;
        public Observable<string> CurrentLocalization => _currentLocalization;
        public IObservableCollection<(Type, string)> RegisteredKeys => _registeredKeys;

        protected readonly ObservableList<string> _availableLocalizations = [];
        protected readonly ReactiveProperty<string> _currentLocalization = new("ERROR");
        protected readonly ObservableHashSet<(Type, string)> _registeredKeys = [];
        private readonly Dictionary<(string, string, Type), object> _localizations = [];

        public LocalizationManager(params LocalizationData[] datas)
        {
            AddLocalization(datas);
            if (_availableLocalizations.Count > 0) { _currentLocalization.Value = _availableLocalizations[0]; }
        }

        public virtual bool RegisterKey<T>(string key)
        {
            Type type = typeof(T);
            bool result = _registeredKeys.Add((type, key));
            return result;
        }
        public void SetLocalization(string localization) { if (_availableLocalizations.Contains(localization)) { _currentLocalization.Value = localization; } }
        public void SetLocalization(int index) { if (index >= 0 && index < _availableLocalizations.Count) { SetLocalization(_availableLocalizations[index]); } }
        public virtual void AddLocalization(LocalizationData data)
        {
            if (!_availableLocalizations.Contains(data.LocalizationKey)) { _availableLocalizations.Add(data.LocalizationKey); }
            foreach (LocalizationPair pair in data.Pairs)
            {
                (string lang, string key, Type type) key = (data.LocalizationKey, pair.Key, pair.Value.GetType());
                _localizations[key] = pair.Value;
            }
        }
        public void AddLocalization(params LocalizationData[] datas) { foreach (var data in datas) { AddLocalization(data); } }

        public T Localize<T>(string key)
        {
            if (!_registeredKeys.Contains((typeof(T), key))) { throw new Exception("Key not registered"); }
            return LocalizeInternal<T>(key);
        }
        protected virtual T LocalizeInternal<T>(string key)
        {
            if (_localizations.TryGetValue((_currentLocalization.CurrentValue, key, typeof(T)), out object? value)) { return (T)value; }
            else { return default!; }
        }
    }
}
