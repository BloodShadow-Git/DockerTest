using System;
using System.Windows.Input;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class LoginPageModel : ViewModelBase
    {
        public BindableReactiveProperty<string> Login { get; } = new();
        public BindableReactiveProperty<string> Password { get; } = new();
        public BindableReactiveProperty<bool> Remember { get; } = new();
        public BindableReactiveProperty<int> HostIndex { get; } = new(0);
        public IReadOnlyObservableList<string> Hosts => _hosts;
        public BindableReactiveProperty<int> UserLoginIndex { get; } = new();
        public IReadOnlyObservableList<string> UserLogins => _userLogins;
        public ICommand LoginCoomand => _loginCommand;

        public IReadOnlyBindableReactiveProperty<string> LoginKey => _loginKey;
        public IReadOnlyBindableReactiveProperty<string> PasswordKey => _passwordKey;
        public IReadOnlyBindableReactiveProperty<string> RememberMeKey => _rememberMeKey;
        public IReadOnlyBindableReactiveProperty<string> EnterKey => _enterKey;
        public BindableReactiveProperty<string> _loginKey = new();
        public BindableReactiveProperty<string> _passwordKey = new();
        public BindableReactiveProperty<string> _rememberMeKey = new();
        public BindableReactiveProperty<string> _enterKey = new();

        private ObservableList<string> _hosts = ["http://localhost:8080", "http://192.168.0.100:8080"];
        private ObservableList<string> _userLogins = ["bloodshadow", "blood-shadow"];
        private ReactiveCommand<Unit> _loginCommand = new();

        public LoginPageModel()
        {
            _loginKey.Value = "login";
            _passwordKey.Value = "password";
            _rememberMeKey.Value = "rememberMe";
            _enterKey.Value = "enter";

            _loginCommand.Subscribe(_ =>
            {
                Console.WriteLine("Enter\nLogin: {0}\nPassword: {1}\nRemember: {2}\nHost: {3}\nUser: {4}",
                    Login.CurrentValue, Password.CurrentValue, Remember.CurrentValue, _hosts[HostIndex.CurrentValue], _userLogins[UserLoginIndex.CurrentValue]);
            });
        }
    }

    public static class LoginPageLocalization
    {
        public const string LOGIN_KEY = nameof(LOGIN_KEY);
        public const string PASSWORD_KEY = nameof(PASSWORD_KEY);
        public const string REMEMBER_ME_KEY = nameof(REMEMBER_ME_KEY);
        public const string ENTER_KEY = nameof(ENTER_KEY);
    }
}