using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class NewLoginPageModel : ViewModelBase
    {
        public BindableReactiveProperty<string> Login { get; } = new();
        public BindableReactiveProperty<string> Password { get; } = new();
        public BindableReactiveProperty<bool> Remember { get; } = new();
        public BindableReactiveProperty<int> HostIndex { get; } = new(0);
        public IReadOnlyObservableList<string> Hosts => _hosts;
        public ICommand LoginCommand => _loginCommand;
        public ICommand RegisterCommand => _registerCommand;
        public BindableReactiveProperty<int> LanguagesIndex { get; } = new();
        public IObservableCollection<string> Languages => LocalizationManager.AvailableLocalizations;
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        public IReadOnlyBindableReactiveProperty<string> RegisterKey => _registerKey;
        public BindableReactiveProperty<string> _loginKey = new();
        public BindableReactiveProperty<string> _passwordKey = new();
        public BindableReactiveProperty<string> _rememberMeKey = new();
        public BindableReactiveProperty<string> _enterKey = new();
        public BindableReactiveProperty<string> _registerKey = new();

        private ObservableList<string> _hosts = ["http://localhost:8080", "http://192.168.0.100:8080"];
        private ReactiveCommand<Unit> _loginCommand = new();
        private ReactiveCommand<Unit> _registerCommand = new();

        public NewLoginPageModel()
        {
            LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _loginKey.Value = LocalizationManager.Localize<string>(PageLocalizations.LOGIN_KEY);
                _passwordKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_KEY);
                _rememberMeKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REMEMBER_ME_KEY);
                _enterKey.Value = LocalizationManager.Localize<string>(PageLocalizations.ENTER_KEY);
                _registerKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REGISTER_KEY);
            });
            LanguagesIndex.Subscribe(_ => { LocalizationManager.SetLocalization(LanguagesIndex.Value); });
            ThemeSwitch.Subscribe(_ => Settings.ThemeSwitcher.Value = ThemeSwitch.Value);

            _loginCommand.Subscribe(_ =>
            {
                Console.WriteLine("Enter\nLogin: {0}\nPassword: {1}\nRemember: {2}\nHost: {3}",
                    Login.CurrentValue, Password.CurrentValue, Remember.CurrentValue, _hosts[HostIndex.CurrentValue]);
                MainWindow.NM.Show(new Notification("File saved", "Your document has been saved successfully.", NotificationType.Success, TimeSpan.FromSeconds(3)));
            });
        }
    }

    public static partial class PageLocalizations
    {
        public const string LOGIN_KEY = nameof(LOGIN_KEY);
        public const string PASSWORD_KEY = nameof(PASSWORD_KEY);
        public const string REMEMBER_ME_KEY = nameof(REMEMBER_ME_KEY);
        public const string ENTER_KEY = nameof(ENTER_KEY);
        public const string REGISTER_KEY = nameof(REGISTER_KEY);
    }
}