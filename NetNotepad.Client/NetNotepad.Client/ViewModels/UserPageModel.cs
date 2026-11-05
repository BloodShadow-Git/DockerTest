using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.Views;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.ViewModels
{
    public partial class UserPageModel : ViewModelBase
    {
        public ObservableList<UserAction> Actions { get; } = [
            new(PageLocalizations.SETTINGS_KEY, "Gear", () => MainWindow.NP.PushAsync(new SettingsPage()).Wait())
        ];

        public UserPageModel()
        {

        }
    }

    public struct UserAction
    {
        public IReadOnlyBindableReactiveProperty<string> ActionName => _actionName;
        public ICommand Action => _action;
        public IReadOnlyBindableReactiveProperty<Bitmap?> Icon => _icon;

        private BindableReactiveProperty<string> _actionName = new();
        private ReactiveCommand<Unit> _action = new();
        private BindableReactiveProperty<Bitmap?> _icon = new();

        public UserAction(string actionNameKey, string iconName, Action? action)
        {
            BindableReactiveProperty<Bitmap?> localIcon = _icon;
            Application.Current!.ActualThemeVariantChanged += (_, _) => { if (Application.Current!.Resources.TryGetResource(iconName, Application.Current!.ActualThemeVariant, out object? value)) { localIcon.Value = (Bitmap)value!; } };
            if (Application.Current!.Resources.TryGetResource(iconName, Application.Current!.ActualThemeVariant, out object? value)) { localIcon.Value = (Bitmap)value!; }
            BindableReactiveProperty<string> localActionName = _actionName;
            LocalizationManager.CurrentLocalization.Subscribe(_ => { localActionName.Value = LocalizationManager.Localize<string>(actionNameKey); });
            _action.Subscribe(_ => action?.Invoke());
        }
    }

    public static partial class PageLocalizations
    {
        public const string SETTINGS_KEY = nameof(SETTINGS_KEY);
    }
}