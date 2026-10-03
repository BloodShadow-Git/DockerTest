using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace NetNotepad.Client.Android;

[Activity(
    Label = "NetNotepad.Client.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
