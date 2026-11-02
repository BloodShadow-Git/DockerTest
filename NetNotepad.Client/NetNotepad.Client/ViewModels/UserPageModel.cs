using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using NetNotepad.Client.Source.Localization;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class UserPageModel : ViewModelBase
    {
        public ObservableList<UserAction> Actions { get; } = [
            new(PageLocalizations.ACCEPT_KEY, "Gear", () => Console.WriteLine("TEST1")),
            new(PageLocalizations.DECLINE_KEY, "Gear", () => Console.WriteLine("TEST2")),
            new(PageLocalizations.HOST_PL, "Theme", () => Console.WriteLine("TEST3")),
            new(PageLocalizations.INVALID_PASSWORD, "Theme", () => Console.WriteLine("TEST4")),
        ];

        public UserPageModel()
        {

        }
    }

    public struct UserAction
    {
        public IReadOnlyBindableReactiveProperty<string> ActionName => _actionName;
        public ICommand Action => _action;
        public IReadOnlyBindableReactiveProperty<IImage?> Icon => _icon;

        private BindableReactiveProperty<string> _actionName = new();
        private ReactiveCommand<Unit> _action = new();
        private BindableReactiveProperty<IImage?> _icon = new();

        public UserAction(string actionNameKey, string iconName, Action? action)
        {
            Application.Current!.ActualThemeVariantChanged += (_, _) => _icon.Value = Application.Current!.FindResource(iconName);
            BindableReactiveProperty<string> localActionName = _actionName;
            LocalizationManager.CurrentLocalization.Subscribe(_ => { localActionName.Value = LocalizationManager.Localize<string>(actionNameKey); });
            _action.Subscribe(_ => action?.Invoke());
        }
    }

    public static partial class PageLocalizations
    {
    }
}