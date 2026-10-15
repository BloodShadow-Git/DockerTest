using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
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
        public NotifyCollectionChangedSynchronizedViewList<string> Hosts => _hosts.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public ICommand LoginCommand => _loginCommand;
        public BindableReactiveProperty<string> LanguagesItem { get; } = new();
        public NotifyCollectionChangedSynchronizedViewList<string> Languages { get; } = LocalizationManager.AvailableLocalizations.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        private BindableReactiveProperty<string> _loginKey { get; } = new();
        private BindableReactiveProperty<string> _passwordKey { get; } = new();
        private BindableReactiveProperty<string> _rememberMeKey { get; } = new();
        private BindableReactiveProperty<string> _enterKey { get; } = new();

        private ObservableList<string> _hosts { get; } = ["http://localhost:8080", "http://192.168.0.100:8080"];
        private ReactiveCommand<Unit> _loginCommand { get; } = new();

        private CompositeDisposable _cd { get; } = [];

        public NewLoginPageModel()
        {
            _cd.Add(Login);
            _cd.Add(Password);
            _cd.Add(Remember);
            _cd.Add(HostIndex);
            _cd.Add(LanguagesItem);
            _cd.Add(Languages);
            _cd.Add(ThemeSwitch);
            _cd.Add(_loginKey);
            _cd.Add(_passwordKey);
            _cd.Add(_rememberMeKey);
            _cd.Add(_enterKey);
            _cd.Add(_loginCommand);

            _cd.Add(LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _loginKey.Value = LocalizationManager.Localize<string>(PageLocalizations.LOGIN_KEY);
                _passwordKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_KEY);
                _rememberMeKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REMEMBER_ME_KEY);
                _enterKey.Value = LocalizationManager.Localize<string>(PageLocalizations.ENTER_KEY);
            }));
            _cd.Add(LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization));
            _cd.Add(ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v));

            _cd.Add(_loginCommand.Subscribe(_ =>
            {
                Console.WriteLine("Enter\nLogin: {0}\nPassword: {1}\nRemember: {2}\nHost: {3}",
                    Login.CurrentValue, Password.CurrentValue, Remember.CurrentValue, _hosts[HostIndex.CurrentValue]);
                MainWindow.NM.Show(new Notification("File saved", "Your document has been saved successfully.", NotificationType.Success, TimeSpan.FromSeconds(3)));
            }));
        }
    }

    public static partial class PageLocalizations
    {
        public const string LOGIN_KEY = nameof(LOGIN_KEY);
        public const string PASSWORD_KEY = nameof(PASSWORD_KEY);
        public const string REMEMBER_ME_KEY = nameof(REMEMBER_ME_KEY);
        public const string ENTER_KEY = nameof(ENTER_KEY);
    }
}