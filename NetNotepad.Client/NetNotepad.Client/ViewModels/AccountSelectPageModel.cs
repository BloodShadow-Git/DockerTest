using System;
using System.Windows.Input;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
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

        private ObservableList<UserProfileView> _userList = [];
        private ReactiveCommand<UserProfileView> _enterCommand = new();
        private ReactiveCommand<UserProfileView> _removeCommand = new();

        public AccountSelectPageModel()
        {
            UserList = _userList.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
            _userList.Add(new(null, null, null, null, null));
            foreach ((_, UserProfile profile) in DataManager.UserList) { _userList.Insert(_userList.Count - 1, profile); }
            DataManager.UserList.ObserveAdd().Subscribe(e => { _userList.Insert(_userList.Count - 1, e.Value.Value); });
            DataManager.UserList.ObserveRemove().Subscribe(e => { _userList.Remove(e.Value.Value); });

            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);
            _enterCommand.Subscribe(Enter);
            _removeCommand.Subscribe(Remove);
        }

        private async void Enter(UserProfileView upv)
        {
            if (upv.UserID.Value.UserGuid == null) { }
            await MainWindow.NP.PushAsync(new NewLoginPage());
        }
        private async void Remove(UserProfileView upv)
        {
            await MainWindow.NP.PushAsync(new UserPage());
        }
    }

    public struct UserProfileView
    {
        public readonly IReadOnlyBindableReactiveProperty<UserIDView> UserID => _userID;
        public readonly IReadOnlyBindableReactiveProperty<string> UserLogin => _userLogin;
        public readonly IReadOnlyBindableReactiveProperty<string> LastLogin => _lastLogin;
        public readonly IReadOnlyBindableReactiveProperty<string> RefreshTokenKey => _refreshTokenKey;
        public readonly IReadOnlyBindableReactiveProperty<string> PasswordDate => _passwordDate;

        private BindableReactiveProperty<UserIDView> _userID = new();
        private BindableReactiveProperty<string> _userLogin = new();
        private BindableReactiveProperty<string> _lastLogin = new();
        private BindableReactiveProperty<string> _refreshTokenKey = new();
        private BindableReactiveProperty<string> _passwordDate = new();

        private DateTime _lastLoginF;
        private DateTime _passwordDateF;

        public UserProfileView(UserIDView? UserID, string? UserLogin, DateTime? LastLogin, string? RefreshTokenKey, DateTime? PasswordDate)
        {
            if (UserID != null) { _userID.Value = (UserIDView)UserID; }
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
            if (PasswordDate != null)
            {
                _passwordDateF = (DateTime)PasswordDate;
                var _passwordDateLocal = _passwordDate;
                LocalizationManager.CurrentLocalization.Subscribe(_ => { _passwordDateLocal.Value = string.Format("{0}: {1:dd.MM.yyyy HH:mm}", LocalizationManager.Localize<string>(PageLocalizations.PASSWORD_REQUIRED_AT), PasswordDate); });
            }
        }

        public static implicit operator UserProfileView(UserProfile? up) => new(up?.UserID, up?.UserLogin, up?.LastLogin, up?.RefreshTokenKey, up?.PasswordDate);
        public static implicit operator UserProfile(UserProfileView? upv) => new(upv?.UserID.Value, upv?.UserLogin.Value!, (DateTime)upv?._lastLoginF!, upv?.RefreshTokenKey.Value!, (DateTime)upv?._passwordDateF!);
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
        public const string PASSWORD_REQUIRED_AT = nameof(PASSWORD_REQUIRED_AT);
    }
}