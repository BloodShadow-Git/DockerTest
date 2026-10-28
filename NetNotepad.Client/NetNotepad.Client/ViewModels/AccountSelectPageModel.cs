using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
using NetNotepad.Contracts;
using ObservableCollections;
using R3;
using static NetNotepad.Client.Source.DataManager;

namespace NetNotepad.Client.ViewModels
{
    public partial class AccountSelectPageModel : ViewModelBase
    {
        public BindableReactiveProperty<string> LanguagesItem { get; } = new(LocalizationManager.CurrentLocalization.CurrentValue);
        public NotifyCollectionChangedSynchronizedViewList<string> Languages { get; } = LocalizationManager.AvailableLocalizations.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();
        public NotifyCollectionChangedSynchronizedViewList<UserProfileView> UserList { get; }
        public ICommand EnterCommand => _enterCommand;
        public ICommand RemoveCommand => _removeCommand;

        public IReadOnlyBindableReactiveProperty<bool> ShowPasswordPanel => _showPasswordPanel;
        public IReadOnlyBindableReactiveProperty<string> AcceptKey => _acceptKey;
        public IReadOnlyBindableReactiveProperty<string> DeclineKey => _declineKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordRequiredKey => _passwordRequiredKey;
        public ICommand AcceptPassword => _acceptPassword;
        public ICommand DeclinePassword => _declinePassword;
        public BindableReactiveProperty<string> Password { get; } = new();


        private ObservableList<UserProfileView> _userList = [];
        private ReactiveCommand<UserProfileView> _enterCommand = new();
        private ReactiveCommand<UserProfileView> _removeCommand = new();
        private BindableReactiveProperty<bool> _showPasswordPanel = new(false);

        private TaskCompletionSource? _passwordTCS;
        private BindableReactiveProperty<string> _acceptKey = new();
        private BindableReactiveProperty<string> _declineKey = new();
        private BindableReactiveProperty<string> _passwordKey = new();
        private BindableReactiveProperty<string> _passwordRequiredKey = new();
        private ReactiveCommand<Unit> _acceptPassword = new();
        private ReactiveCommand<Unit> _declinePassword = new();

        public AccountSelectPageModel()
        {
            LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _acceptKey.Value = LocalizationManager.Localize<string>(PageLocalizations.ACCEPT_KEY);
                _declineKey.Value = LocalizationManager.Localize<string>(PageLocalizations.DECLINE_KEY);
                _passwordRequiredKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_REQUIRED);
                _passwordKey.Value = LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_KEY);
            });

            UserList = _userList.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
            _userList.Add(new(null, null, null, null, null));
            foreach ((_, UserProfile profile) in DataManager.UserList) { _userList.Insert(_userList.Count - 1, profile); }
            DataManager.UserList.ObserveAdd().Subscribe(e => { _userList.Insert(_userList.Count - 1, e.Value.Value); });
            DataManager.UserList.ObserveRemove().Subscribe(e => { _userList.Remove(_userList.FirstOrDefault(x => x.UserID.Value.UserGuid.Value == e.Value.Value.UserID.UserGuid)); });
            DataManager.UserList.ObserveReplace().Subscribe(e =>
            {
                int index = _userList.IndexOf(_userList.FirstOrDefault(d => d.UserID.Value.UserGuid.Value == e.OldValue.Key.UserGuid));
                if (index < 0) { return; }
                _userList[index] = e.NewValue.Value;
            });

            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);
            _enterCommand.SubscribeAwait(async (e, _) => { await Enter(e); }, AwaitOperation.Drop);
            _removeCommand.SubscribeAwait(async (e, _) => { await Remove(e); }, AwaitOperation.Drop);

            _acceptPassword.Subscribe(_ => { _passwordTCS?.TrySetResult(); });
            _declinePassword.Subscribe(_ => { _passwordTCS?.TrySetCanceled(); });
        }

        private async Task Enter(UserProfileView upv)
        {
            if (upv.UserID.Value.UserGuid == null)
            {
                await MainWindow.NP.PushAsync(new NewLoginPage());
                return;
            }
            Uri userUri = new(upv.UserID.Value.UserServer.Value); ;
            APIWorker.BaseUrl = userUri;
            if (!await APIWorker.ServerAvailable(userUri)) { MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(PageLocalizations.SERVER_UNAVAILABLE), null, NotificationType.Error)); return; }
            LoginResponse lr = await APIWorker.Login(new LoginRTRequest(upv.UserLogin.Value, upv.RefreshTokenKey.Value, APIWorker.CLIENT));
            if (lr.HttpCode != HttpStatusCode.OK)
            {
                Response resp = await APIWorker.UserExists(new UserExistsRequest(upv.UserLogin.Value));
                if (resp.HttpCode != HttpStatusCode.OK)
                {
                    MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(ServiceExceptions.USER_NOT_EXISTS.Message), null, NotificationType.Error));
                    await Remove(upv);
                    return;
                }
                while (true)
                {
                    Password.Value = "";
                    Task passwordTask = AskPassword();
                    try { await passwordTask; } catch { }
                    _showPasswordPanel.Value = false;
                    if (passwordTask.IsCanceled) { return; }
                    lr = await APIWorker.Login(new LoginRequest(upv.UserLogin.Value, Password.Value, true, APIWorker.CLIENT));
                    if (lr.HttpCode == HttpStatusCode.OK)
                    {
                        MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(PageLocalizations.SUCCESS_LOGIN_KEY), null, NotificationType.Success));
                        break;
                    }
                    MainWindow.NM.Show(new Notification(LocalizationManager.Localize<string>(PageLocalizations.INVALID_PASSWORD), null, NotificationType.Error));
                }
            }
            DataManager.UpdateUser(new(upv.UserID.Value, upv.UserLogin.Value, DateTime.Now, lr.RefreshToken, lr.DeviceGuid));
            DataManager.SelectUser(upv.UserID.Value);
            await MainWindow.NP.PopToRootAsync();
            MainWindow.NP.InsertPage(new UserPage(), MainWindow.NP.CurrentPage!);
        }
        private async Task Remove(UserProfileView upv)
        {
            if (upv.UserID.Value.UserGuid == null)
            {
                await MainWindow.NP.PushAsync(new NewLoginPage());
                return;
            }
            APIWorker.BaseUrl = new(upv.UserID.Value.UserServer.Value);
            LoginResponse lr = await APIWorker.Login(new LoginRTRequest(upv.UserLogin.Value, upv.RefreshTokenKey.Value, APIWorker.CLIENT));
            if (lr.HttpCode == HttpStatusCode.OK) { await APIWorker.Logout(new LogoutRTRequest(lr.JWT, upv.DeviceGuid)); }
            DataManager.RemoveUser(upv.UserID.Value);
        }

        private Task AskPassword()
        {
            _showPasswordPanel.Value = true;
            _passwordTCS = new();
            return _passwordTCS.Task;
        }
    }

    public struct UserProfileView
    {
        public readonly IReadOnlyBindableReactiveProperty<UserIDView> UserID => _userID;
        public readonly IReadOnlyBindableReactiveProperty<string> UserLogin => _userLogin;
        public readonly IReadOnlyBindableReactiveProperty<string> LastLogin => _lastLogin;
        public readonly IReadOnlyBindableReactiveProperty<string> RefreshTokenKey => _refreshTokenKey;
        public readonly IReadOnlyBindableReactiveProperty<bool> Available => _available;
        public readonly IReadOnlyBindableReactiveProperty<bool> ShowLoading => _showLoading;

        private BindableReactiveProperty<UserIDView> _userID = new();
        private BindableReactiveProperty<string> _userLogin = new();
        private BindableReactiveProperty<string> _lastLogin = new();
        private BindableReactiveProperty<string> _refreshTokenKey = new();
        private BindableReactiveProperty<bool> _available = new(false);
        private BindableReactiveProperty<bool> _showLoading = new(false);

        public readonly Guid DeviceGuid;

        private DateTime _lastLoginF;

        public UserProfileView(UserIDView? UserID, string? UserLogin, DateTime? LastLogin, string? RefreshTokenKey, Guid? DeviceGuid)
        {
            if (UserID == null && UserLogin == null && LastLogin == null && RefreshTokenKey == null && DeviceGuid == null) { _available.Value = true; }
            if (UserID != null)
            {
                _userID.Value = (UserIDView)UserID;
                BindableReactiveProperty<bool> localAvailable = _available;
                BindableReactiveProperty<bool> localShowLoading = _showLoading;
                Task.Run(async () =>
                {
                    localShowLoading.Value = true;
                    localAvailable.Value = await APIWorker.ServerAvailable(UserID.Value.UserServer.Value);
                    localShowLoading.Value = false;
                });
            }
            if (UserLogin != null) { _userLogin.Value = UserLogin; }
            else
            {
                var _userLoginLocal = _userLogin;
                LocalizationManager.CurrentLocalization.Subscribe(_ => { _userLoginLocal.Value = LocalizationManager.Localize<string>(PageLocalizations.ADD_USER); });
            }
            if (LastLogin != null)
            {
                _lastLoginF = (DateTime)LastLogin;
                var _lastLoginLocal = _lastLogin;
                LocalizationManager.CurrentLocalization.Subscribe(_ => { _lastLoginLocal.Value = string.Format("{0}: {1:dd.MM.yyyy HH:mm}", LocalizationManager.Localize<string>(PageLocalizations.LAST_LOGIN), LastLogin); });
            }
            if (RefreshTokenKey != null) { _refreshTokenKey.Value = RefreshTokenKey; }
            if (DeviceGuid != null) { this.DeviceGuid = (Guid)DeviceGuid; }
        }

        public static implicit operator UserProfileView(UserProfile? up) => new(up?.UserID, up?.UserLogin, up?.LastLogin, up?.RefreshTokenKey, up?.DeviceGuid);
        public static implicit operator UserProfile(UserProfileView? upv) => new(upv?.UserID.Value, upv?.UserLogin.Value!, (DateTime)upv?._lastLoginF!, upv?.RefreshTokenKey.Value!, (Guid)upv?.DeviceGuid!);
    }
    public struct UserIDView
    {
        public readonly IReadOnlyBindableReactiveProperty<Guid> UserGuid => _userGuid;
        public readonly IReadOnlyBindableReactiveProperty<string> UserServer => _userServer;
        private BindableReactiveProperty<Guid> _userGuid = new();
        private BindableReactiveProperty<string> _userServer = new();

        public UserIDView(Guid? UserGuid, string? UserServer)
        {
            if (UserGuid != null) { _userGuid.Value = (Guid)UserGuid; }
            if (UserServer != null) { _userServer.Value = UserServer; }
        }

        public static implicit operator UserIDView(UserID? id) => new(id?.UserGuid, id?.UserServer);
        public static implicit operator UserID(UserIDView? idv) => new((Guid)idv?.UserGuid.Value!, idv?.UserServer.Value!);
    }

    public static partial class PageLocalizations
    {
        public const string ADD_USER = nameof(ADD_USER);
        public const string LAST_LOGIN = nameof(LAST_LOGIN);
        public const string PASSWORD_REQUIRED = nameof(PASSWORD_REQUIRED);
        public const string INVALID_PASSWORD = nameof(INVALID_PASSWORD);
        public const string ACCEPT_KEY = nameof(ACCEPT_KEY);
        public const string DECLINE_KEY = nameof(DECLINE_KEY);
    }
}