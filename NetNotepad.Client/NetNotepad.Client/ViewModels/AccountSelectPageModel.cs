using System.Windows.Input;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class AccountSelectPageModel : ViewModelBase
    {
        public BindableReactiveProperty<int> LanguagesIndex { get; } = new();
        public IObservableCollection<string> Languages { get; } = LocalizationManager.AvailableLocalizations;
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = Settings.ThemeSwitcher.ToBindableReactiveProperty();
        public ICommand TestCommand => _testCommand;

        private ReactiveCommand<Unit> _testCommand = new();

        public AccountSelectPageModel()
        {
            LanguagesIndex.Subscribe(_ => { LocalizationManager.SetLocalization(LanguagesIndex.Value); });
            ThemeSwitch.Subscribe(_ => Settings.ThemeSwitcher.Value = ThemeSwitch.Value);
            _testCommand.Subscribe(async _ =>
            {
                await MainWindow.NP.PushAsync(new NewLoginPage());
            });
        }
    }

    public static partial class PageLocalizations
    {
    }
}