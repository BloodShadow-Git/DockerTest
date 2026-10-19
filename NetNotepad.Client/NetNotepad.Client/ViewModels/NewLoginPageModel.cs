using System;
using System.Linq;
using System.Net;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
using NetNotepad.Contracts;
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
        public ICommand RegisterCommand => _registerCommand;

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        public IReadOnlyBindableReactiveProperty<string> HostPL => _hostPL;
        public IReadOnlyBindableReactiveProperty<string> RegisterKey => _registerKey;
        private BindableReactiveProperty<string> _loginKey { get; } = new();
        private BindableReactiveProperty<string> _passwordKey { get; } = new();
        private BindableReactiveProperty<string> _rememberMeKey { get; } = new();
        private BindableReactiveProperty<string> _enterKey { get; } = new();
        private BindableReactiveProperty<string> _hostPL { get; } = new();
        private BindableReactiveProperty<string> _registerKey { get; } = new();

        private ReactiveCommand<Unit> _loginCommand { get; } = new();
        private ReactiveCommand<Unit> _registerCommand { get; } = new();

        public NewLoginPageModel()
        {
            LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _loginKey.Value = LocalizationManager.Localize<string>(PageLocalizations.LOGIN_KEY);
                _passwordKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_KEY);
                _rememberMeKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REMEMBER_ME_KEY);
                _enterKey.Value = LocalizationManager.Localize<string>(PageLocalizations.ENTER_KEY);
                _hostPL.Value = LocalizationManager.Localize<string>(PageLocalizations.HOST_PL);
                _registerKey.Value = LocalizationManager.Localize<string>(PageLocalizations.REGISTER_KEY);
            });
            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);

            _loginCommand.Subscribe(async _ =>
            {
                try
                {
                    if (string.IsNullOrEmpty(Login.Value) ||
                        string.IsNullOrEmpty(Password.Value) ||
                        string.IsNullOrEmpty(Host.Value))
                    {
                        MainWindow.NM.Show("Please fill all fields", NotificationType.Error);
                        return;
                    }
                    APIWorker.BaseUrl = new(Host.Value);
                    LoginResponse lr = await APIWorker.Login(new LoginRequest(Login.Value, Password.Value, Remember.Value, APIWorker.CLIENT));
                    if (lr.HttpCode != HttpStatusCode.OK)
                    {
                        MainWindow.NM.Show(new Notification(lr.Message, null, NotificationType.Error, TimeSpan.FromSeconds(3)));
                        return;
                    }
                    UserGetResponse ugr = await APIWorker.GetUser(new(lr.JWT));
                    if (ugr.HttpCode != HttpStatusCode.OK)
                    {
                        MainWindow.NM.Show(new Notification(ugr.Message, null, NotificationType.Error, TimeSpan.FromSeconds(3)));
                        return;
                    }
                    DataManager.UserID userid = new(ugr.UserGuid, Host.Value);
                    if (DataManager.UserList.ContainsKey(userid)) { DataManager.RemoveUser(userid); }
                    DataManager.AddUser(new(userid, ugr.UserLogin, DateTime.Now, lr.RefreshToken, lr.CreationDate + ugr.RefreshTokenTTL));
                }
                catch (Exception ex) { MainWindow.NM.Show(new Notification("Exception", ex.ToString(), NotificationType.Error)); }
            });
            _registerCommand.Subscribe(async _ =>
            {
                try
                {
                    if (string.IsNullOrEmpty(Login.Value) ||
                        string.IsNullOrEmpty(Password.Value) ||
                        string.IsNullOrEmpty(Host.Value))
                    {
                        MainWindow.NM.Show("Please fill all fields", NotificationType.Error);
                        return;
                    }
                    APIWorker.BaseUrl = new(Host.Value);
                    RegisterResponse rr = await APIWorker.Register(new RegisterRequest(Login.Value, Password.Value, Remember.Value, APIWorker.CLIENT));
                    if (rr.HttpCode != HttpStatusCode.OK)
                    {
                        MainWindow.NM.Show(new Notification(rr.Message, null, NotificationType.Error, TimeSpan.FromSeconds(3)));
                        return;
                    }
                    UserGetResponse ugr = await APIWorker.GetUser(new(rr.JWT));
                    if (ugr.HttpCode != HttpStatusCode.OK)
                    {
                        MainWindow.NM.Show(new Notification(ugr.Message, null, NotificationType.Error, TimeSpan.FromSeconds(3)));
                        return;
                    }
                    DataManager.UserID userid = new(ugr.UserGuid, Host.Value);
                    if (DataManager.UserList.ContainsKey(userid)) { DataManager.RemoveUser(userid); }
                    DataManager.AddUser(new(userid, ugr.UserLogin, DateTime.Now, rr.RefreshToken, rr.CreationDate + ugr.RefreshTokenTTL));
                }
                catch (Exception ex) { MainWindow.NM.Show(new Notification("Exception", ex.ToString(), NotificationType.Error)); }
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
        public const string REGISTER_KEY = nameof(REGISTER_KEY);
    }
}