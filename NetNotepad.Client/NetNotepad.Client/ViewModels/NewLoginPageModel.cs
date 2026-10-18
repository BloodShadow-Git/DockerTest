using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
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
        public BindableReactiveProperty<string> Host { get; } = new();
        public NotifyCollectionChangedSynchronizedViewList<string> Hosts => DataManager.HostList.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public ICommand LoginCommand => _loginCommand;
        public BindableReactiveProperty<string> LanguagesItem { get; } = new(LocalizationManager.CurrentLocalization.CurrentValue);
        public NotifyCollectionChangedSynchronizedViewList<string> Languages { get; } = LocalizationManager.AvailableLocalizations.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        public IReadOnlyBindableReactiveProperty<string> HostPL => _hostPL;
        private BindableReactiveProperty<string> _loginKey { get; } = new();
        private BindableReactiveProperty<string> _passwordKey { get; } = new();
        private BindableReactiveProperty<string> _rememberMeKey { get; } = new();
        private BindableReactiveProperty<string> _enterKey { get; } = new();
        private BindableReactiveProperty<string> _hostPL { get; } = new();

        private ReactiveCommand<Unit> _loginCommand { get; } = new();

        public NewLoginPageModel()
        {
            LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _loginKey.Value = LocalizationManager.Localize<string>(PageLocalizations.LOGIN_KEY);
                _passwordKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_KEY);
                _rememberMeKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REMEMBER_ME_KEY);
                _enterKey.Value = LocalizationManager.Localize<string>(PageLocalizations.ENTER_KEY);
                _hostPL.Value = LocalizationManager.Localize<string>(PageLocalizations.HOST_PL);
            });
            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);

            _loginCommand.Subscribe(_ =>
            {
                Console.WriteLine("Enter\nLogin: {0}\nPassword: {1}\nRemember: {2}\nHost: {3}",
                    Login.CurrentValue, Password.CurrentValue, Remember.CurrentValue, Host.CurrentValue);
                MainWindow.NM.Show(new Notification("Client", $"{Application.Current!.Name} ({RuntimeInformation.OSDescription})", NotificationType.Success, TimeSpan.FromSeconds(3)));
            });
        }
    }

    public static partial class PageLocalizations
    {
        public const string LOGIN_KEY = nameof(LOGIN_KEY);
        public const string PASSWORD_KEY = nameof(PASSWORD_KEY);
        public const string REMEMBER_ME_KEY = nameof(REMEMBER_ME_KEY);
        public const string ENTER_KEY = nameof(ENTER_KEY);
        public const string HOST_PL = nameof(HOST_PL);
    }
}