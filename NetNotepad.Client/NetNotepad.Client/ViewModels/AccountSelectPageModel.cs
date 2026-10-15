using System;
using System.Windows.Input;
using Avalonia.Platform;
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
        public BindableReactiveProperty<string> LanguagesItem { get; } = new();
        public NotifyCollectionChangedSynchronizedViewList<string> Languages { get; } = LocalizationManager.AvailableLocalizations.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();
        // public NotifyCollectionChangedSynchronizedViewList<UserProfileView> UserList { get; } = DataManager.UserList.CreateView<UserProfileView>(e => e.Value).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        public NotifyCollectionChangedSynchronizedViewList<UserProfileView> UserList { get; }
        public ICommand TestCommand => _testCommand;
        public ICommand TostCommand => _tostCommand;

        private ObservableList<UserProfileView> _userList = [];
        private ReactiveCommand<Unit> _testCommand = new();
        private ReactiveCommand<Unit> _tostCommand = new();

        public AccountSelectPageModel()
        {
            _userList = [];
            UserList = _userList.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
            _userList.Add(new(null!, null!, DateTime.MinValue, null!, DateTime.MinValue));
            foreach ((_, UserProfile profile) in DataManager.UserList) { _userList.Add(profile); }
            DataManager.UserList.ObserveAdd().Subscribe(e => { _userList.Insert(_userList.Count - 1, e.Value.Value); });
            DataManager.UserList.ObserveRemove().Subscribe(e => { _userList.Remove(e.Value.Value); });

            LanguagesItem.Skip(1).Subscribe(LocalizationManager.SetLocalization);
            ThemeSwitch.Subscribe(v => Settings.ThemeSwitcher.Value = v);
            _testCommand.Subscribe(async _ =>
            {
                await MainWindow.NP.PushAsync(new NewLoginPage());
            });
            _tostCommand.Subscribe(async _ =>
            {
                await MainWindow.NP.PushAsync(new UserPage());
            });
        }
    }

    public struct UserProfileView
    {
        public readonly IReadOnlyBindableReactiveProperty<UserIDView> UserID => _userID;
        public readonly IReadOnlyBindableReactiveProperty<string> UserLogin => _userLogin;
        public readonly IReadOnlyBindableReactiveProperty<DateTime> LastLogin => _lastLogin;
        public readonly IReadOnlyBindableReactiveProperty<string> RefreshTokenKey => _refreshTokenKey;
        public readonly IReadOnlyBindableReactiveProperty<DateTime> PasswordDate => _passwordDate;

        private BindableReactiveProperty<UserIDView> _userID = new();
        private BindableReactiveProperty<string> _userLogin = new();
        private BindableReactiveProperty<DateTime> _lastLogin = new();
        private BindableReactiveProperty<string> _refreshTokenKey = new();
        private BindableReactiveProperty<DateTime> _passwordDate = new();

        public UserProfileView(UserIDView? UserID, string? UserLogin, DateTime? LastLogin, string? RefreshTokenKey, DateTime? PasswordDate)
        {
            if (UserID != null) { _userID.Value = (UserIDView)UserID; }
            if (UserLogin != null) { _userLogin.Value = UserLogin; }
            if (LastLogin != null) { _lastLogin.Value = (DateTime)LastLogin; }
            if (RefreshTokenKey != null) { _refreshTokenKey.Value = RefreshTokenKey; }
            if (PasswordDate != null) { _passwordDate.Value = (DateTime)PasswordDate; }
        }

        public static implicit operator UserProfileView(UserProfile? up) => new(up?.UserID, up?.UserLogin, up?.LastLogin, up?.RefreshTokenKey, up?.PasswordDate);
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
    }

    public static partial class PageLocalizations
    {
    }
}