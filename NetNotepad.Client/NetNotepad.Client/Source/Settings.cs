using Avalonia;
using Avalonia.Styling;
using NetNotepad.Client.Source.Localization;
using R3;

namespace NetNotepad.Client.Source
{
    public static class Settings
    {
        public static ReactiveProperty<bool> ThemeSwitcherBool { get; } = new();
        public static ReactiveProperty<ThemeVariant> ThemeSwitcher { get; } = new();
        public static ReactiveProperty<string> CurrentLanguage { get; } = new();
        public static ReactiveProperty<int> UpdateTimer { get; } = new(5);

        private const string SETTINGS_FILE = "settings.json";

        static Settings()
        {
            ThemeSwitcher.Subscribe(_ => Application.Current!.RequestedThemeVariant = ThemeSwitcher.Value);
            ThemeSwitcherBool.Value = ConvertToBool(Application.Current!.RequestedThemeVariant!);
            ThemeSwitcherBool.Subscribe(_ =>
            {
                if (ThemeSwitcherBool.CurrentValue) { ThemeSwitcher.Value = ThemeVariant.Light; }
                else { ThemeSwitcher.Value = ThemeVariant.Dark; }
            });
            LocalizationManager.CurrentLocalization.Subscribe(_ => { CurrentLanguage.Value = LocalizationManager.CurrentLocalization.CurrentValue; });
            CurrentLanguage.Subscribe(_ => { LocalizationManager.SetLocalization(CurrentLanguage.Value); });
            Load();
            ThemeSwitcher.AsUnitObservable()
                .Merge(CurrentLanguage.AsUnitObservable())
                .Subscribe(_ => Save());
        }
        private record SettingsData(ThemeVariant Theme, string Language, int Timer);

        private static bool ConvertToBool(ThemeVariant theme)
        {
            if (theme == ThemeVariant.Light) { return true; }
            else if (theme == ThemeVariant.Dark) { return false; }
            else { return false; }
        }

        private static void Load()
        {
            SettingsData? data = FileSystem.Load<SettingsData>(SETTINGS_FILE);
            if (data == null) { return; }
            ThemeSwitcher.Value = data.Theme;
            CurrentLanguage.Value = data.Language;
            UpdateTimer.Value = data.Timer;
        }

        private static void Save()
        {
            FileSystem.Save(SETTINGS_FILE, new SettingsData(ThemeSwitcher.Value,
                                                            CurrentLanguage.Value,
                                                            UpdateTimer.Value));
        }
    }
}