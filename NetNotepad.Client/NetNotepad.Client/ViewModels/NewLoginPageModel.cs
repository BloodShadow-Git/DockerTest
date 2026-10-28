using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using Extensions;
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
        public IReadOnlyBindableReactiveProperty<bool> IsWaiting => _isWaiting;

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        public IReadOnlyBindableReactiveProperty<string> HostPL => _hostPL;
        public IReadOnlyBindableReactiveProperty<string> RegisterKey => _registerKey;
        public IReadOnlyBindableReactiveProperty<string> ProcessingKey => _processingKey;


        private BindableReactiveProperty<string> _loginKey { get; } = new();
        private BindableReactiveProperty<string> _passwordKey { get; } = new();
        private BindableReactiveProperty<string> _rememberMeKey { get; } = new();
        private BindableReactiveProperty<string> _enterKey { get; } = new();
        private BindableReactiveProperty<string> _hostPL { get; } = new();
        private BindableReactiveProperty<string> _registerKey { get; } = new();
        private BindableReactiveProperty<string> _processingKey { get; } = new();
        public BindableReactiveProperty<bool> _isWaiting { get; } = new();

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
                _processingKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PROCESSING_KEY);
            });
            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);

            Login.EnableValidation(_ => string.IsNullOrWhiteSpace(Login.Value) ? new ValidationException(LocalizationManager.Localize<string>(PageLocalizations.FIELD_MUST_BE_FILLED_KEY)) : null);
            Password.EnableValidation(_ => string.IsNullOrWhiteSpace(Password.Value) ? new ValidationException(LocalizationManager.Localize<string>(PageLocalizations.FIELD_MUST_BE_FILLED_KEY)) : null);
            Host.EnableValidation(_ =>
            {
                if (string.IsNullOrWhiteSpace(Host.Value)) { return new ValidationException(LocalizationManager.Localize<string>(PageLocalizations.FIELD_MUST_BE_FILLED_KEY)); }
                if (!Uri.TryCreate(Host.Value, UriKind.RelativeOrAbsolute, out Uri? _)) { return new ValidationException(LocalizationManager.Localize<string>(PageLocalizations.INVALID_SERVER_ADDRESS)); }
                return null;
            });

            Login.Merge(Password).Merge(Host).Subscribe(_ =>
            {
                bool valid = !string.IsNullOrEmpty(Login.Value) && !string.IsNullOrEmpty(Password.Value) && !string.IsNullOrEmpty(Host.Value);
                _loginCommand.ChangeCanExecute(valid);
                _registerCommand.ChangeCanExecute(valid);
            });

            _loginCommand.SubscribeAwait(async (_, ct) => await AuthenticateAsync(async () =>
            {
                LoginResponse lr = await APIWorker.Login(new LoginRequest(Login.Value, Password.Value, Remember.Value, APIWorker.CLIENT));
                return new AuthResult(lr.HttpCode, lr.Message, lr.JWT, lr.RefreshToken, lr.CreationDate, lr.DeviceGuid);
            }, PageLocalizations.SUCCESS_LOGIN_KEY), AwaitOperation.Drop);
            _registerCommand.SubscribeAwait(async (_, ct) => await AuthenticateAsync(async () =>
            {
                RegisterResponse rr = await APIWorker.Register(new RegisterRequest(Login.Value, Password.Value, Remember.Value, APIWorker.CLIENT));
                return new AuthResult(rr.HttpCode, rr.Message, rr.JWT, rr.RefreshToken, rr.CreationDate, rr.DeviceGuid);
            }, PageLocalizations.SUCCESS_REGISTER_KEY), AwaitOperation.Drop);
        }

        private async Task AuthenticateAsync(Func<Task<AuthResult>> authCall, string successKey)
        {
            try
            {
                if (string.IsNullOrEmpty(Login.Value) ||
                    string.IsNullOrEmpty(Password.Value) ||
                    string.IsNullOrEmpty(Host.Value))
                {
                    MainWindow.NM.Show(LocalizationManager.Localize<string>(PageLocalizations.PLEASE_FILL_ALL_FIELDS_KEY), NotificationType.Error);
                    return;
                }
                _isWaiting.Value = true;
                Uri userUri;
                try { userUri = new(Host.Value); }
                catch
                {
                    MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(PageLocalizations.INVALID_SERVER_ADDRESS), null, NotificationType.Error));
                    _isWaiting.Value = false;
                    return;
                }
                if (!await APIWorker.ServerAvailable(userUri))
                {
                    MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(PageLocalizations.SERVER_UNAVAILABLE), null, NotificationType.Error));
                    _isWaiting.Value = false;
                    return;
                }
                APIWorker.BaseUrl = userUri;
                AuthResult auth = await authCall();
                if (auth.HttpCode != HttpStatusCode.OK) { auth.Show(NotificationType.Error); return; }
                UserGetResponse ugr = await APIWorker.GetUser(new(auth.JWT));
                if (ugr.HttpCode != HttpStatusCode.OK) { auth.Show(NotificationType.Error); return; }

                DataManager.UserID userid = new(ugr.UserGuid, Host.Value);
                if (DataManager.UserList.ContainsKey(userid)) { DataManager.RemoveUser(userid); }
                DataManager.UserProfile profile = new(userid, ugr.UserLogin, DateTime.Now, auth.RefreshToken, auth.DeviceGuid);
                DataManager.AddUser(profile);

                MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(successKey), null, NotificationType.Success));
                DataManager.SelectUser(profile.UserID);
                await MainWindow.NP.PopToRootAsync();
                MainWindow.NP.InsertPage(new UserPage(), MainWindow.NP.CurrentPage!);
            }
            catch (Exception ex) { MainWindow.NM.Show(new Notification("Exception", ex.ToString(), NotificationType.Error)); }
            _isWaiting.Value = false;
        }

        private record AuthResult(HttpStatusCode HttpCode, string Message, string JWT, string RefreshToken, DateTime CreationDate, Guid DeviceGuid) : Response(HttpCode, Message);
    }

    public static partial class PageLocalizations
    {
        public const string LOGIN_KEY = nameof(LOGIN_KEY);
        public const string PASSWORD_KEY = nameof(PASSWORD_KEY);
        public const string REMEMBER_ME_KEY = nameof(REMEMBER_ME_KEY);
        public const string ENTER_KEY = nameof(ENTER_KEY);
        public const string HOST_PL = nameof(HOST_PL);
        public const string REGISTER_KEY = nameof(REGISTER_KEY);
        public const string SUCCESS_LOGIN_KEY = nameof(SUCCESS_LOGIN_KEY);
        public const string SUCCESS_REGISTER_KEY = nameof(SUCCESS_REGISTER_KEY);
        public const string PLEASE_FILL_ALL_FIELDS_KEY = nameof(PLEASE_FILL_ALL_FIELDS_KEY);
        public const string FIELD_MUST_BE_FILLED_KEY = nameof(FIELD_MUST_BE_FILLED_KEY);
        public const string INVALID_SERVER_ADDRESS = nameof(INVALID_SERVER_ADDRESS);
        public const string SERVER_UNAVAILABLE = nameof(SERVER_UNAVAILABLE);
        public const string PROCESSING_KEY = nameof(PROCESSING_KEY);
    }
}