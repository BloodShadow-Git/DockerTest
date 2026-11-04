using Avalonia.Styling;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class SettingsPageModel : ViewModelBase
    {
        public IReadOnlyBindableReactiveProperty<string> ThemeKey => _themeKey;
        public IReadOnlyBindableReactiveProperty<string> CurrentLanguageKey => _currentLanguageKey;
        public IReadOnlyBindableReactiveProperty<string> TimerKey => _timerKey;
        public IReadOnlyBindableReactiveProperty<string> SecKey => _secKey;

        public IReadOnlyObservableList<string> ThemeValues => _themeValues;
        public BindableReactiveProperty<int> ThemeValue { get; } = new();
        public BindableReactiveProperty<int> TimerValue { get; } = new();
        public BindableReactiveProperty<string> LanguagesItem { get; } = new(LocalizationManager.CurrentLocalization.CurrentValue);
        public NotifyCollectionChangedSynchronizedViewList<string> Languages { get; } = LocalizationManager.AvailableLocalizations.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        private ObservableList<string> _themeValues = [
            LocalizationManager.Localize<string>(PageLocalizations.SYSTEM_THEME),
            LocalizationManager.Localize<string>(PageLocalizations.LIGHT_THEME),
            LocalizationManager.Localize<string>(PageLocalizations.DARK_THEME)
        ];
        private BindableReactiveProperty<string> _themeKey = new();
        private BindableReactiveProperty<string> _currentLanguageKey = new();
        private BindableReactiveProperty<string> _timerKey = new();
        private BindableReactiveProperty<string> _secKey = new();

        public SettingsPageModel()
        {
            Settings.ThemeSwitcher.Subscribe(_ => ThemeValue.Value = Convert(Settings.ThemeSwitcher.Value));
            ThemeValue.Subscribe(_ => Settings.ThemeSwitcher.Value = Convert(ThemeValue.Value));
            Settings.UpdateTimer.Subscribe(_ => TimerValue.Value = Settings.UpdateTimer.Value);
            TimerValue.Subscribe(_ => Settings.UpdateTimer.Value = TimerValue.Value);
            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);

            LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _themeKey.Value = LocalizationManager.Localize<string>(PageLocalizations.THEME_KEY);
                _currentLanguageKey.Value = LocalizationManager.Localize<string>(PageLocalizations.CURRENT_LANGUAGE_KEY);
                _timerKey.Value = LocalizationManager.Localize<string>(PageLocalizations.TIMER_KEY);
                _secKey.Value = LocalizationManager.Localize<string>(PageLocalizations.SEC_KEY);
            });
        }

        private static int Convert(ThemeVariant variant)
        {
            if (variant == ThemeVariant.Default) { return 0; }
            else if (variant == ThemeVariant.Light) { return 1; }
            else if (variant == ThemeVariant.Dark) { return 2; }
            else { return -1; }
        }

        private static ThemeVariant Convert(int value)
        {
            return value switch
            {
                1 => ThemeVariant.Light,
                2 => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
        }
    }

    public partial class PageLocalizations
    {
        public const string THEME_KEY = nameof(THEME_KEY);
        public const string SYSTEM_THEME = nameof(SYSTEM_THEME);
        public const string LIGHT_THEME = nameof(LIGHT_THEME);
        public const string DARK_THEME = nameof(DARK_THEME);
        public const string CURRENT_LANGUAGE_KEY = nameof(CURRENT_LANGUAGE_KEY);
        public const string TIMER_KEY = nameof(TIMER_KEY);
        public const string SEC_KEY = nameof(SEC_KEY);
    }
}