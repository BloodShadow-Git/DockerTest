using Avalonia;
using Avalonia.Styling;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class AccountSelectPageModel : ViewModelBase
    {
        public BindableReactiveProperty<int> LanguagesIndex { get; } = new();
        public IReadOnlyObservableList<string> Languages => _languages;
        public BindableReactiveProperty<bool> ThemeSwitch { get; } = new();

        private ObservableList<string> _languages = ["Russian", "English"];

        public AccountSelectPageModel()
        {
            ThemeSwitch.Subscribe(_ =>
            {
                if (ThemeSwitch.CurrentValue) { Application.Current!.RequestedThemeVariant = ThemeVariant.Light; }
                else { Application.Current!.RequestedThemeVariant = ThemeVariant.Dark; }
            });
        }
    }

    public static partial class PageLocalizations
    {
    }
}