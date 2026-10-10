using Avalonia;
using Avalonia.Styling;
using R3;

namespace NetNotepad.Client.Source
{
    public static class Settings
    {
        public static ReactiveProperty<bool> ThemeSwitcher { get; } = new();

        static Settings()
        {
            ThemeSwitcher.Value = ConvertToBool(Application.Current!.RequestedThemeVariant!);
            ThemeSwitcher.Subscribe(_ =>
            {
                if (ThemeSwitcher.CurrentValue) { Application.Current!.RequestedThemeVariant = ThemeVariant.Light; }
                else { Application.Current!.RequestedThemeVariant = ThemeVariant.Dark; }
            });
        }
        private record SettingsData(int LocalizationIndex);

        private static bool ConvertToBool(ThemeVariant theme)
        {
            if (theme == ThemeVariant.Light) { return true; }
            else if (theme == ThemeVariant.Dark) { return false; }
            else { return false; }
        }
    }
}