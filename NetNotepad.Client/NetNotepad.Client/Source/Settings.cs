using Avalonia;
using Avalonia.Styling;

namespace NetNotepad.Client.Source
{
    public static class Settings
    {
        static Settings()
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        }

        public static void Init() { }
        private record SettingsData(int LocalizationIndex);
    }
}