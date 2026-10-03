using System;
using System.Diagnostics;
using Avalonia;
using NetNotepad.Client.Source;

namespace NetNotepad.Client.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder ab = AppBuilder.Configure<App>().UsePlatformDetect();

        if (OperatingSystem.IsLinux() &&
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null) { ab.UseWayland(); }

        ab.WithInterFont().LogToTrace().UseR3(ex => Debug.WriteLine(ex));

        if (OperatingSystem.IsWindows()) { TokenStore.Instance = new WindowsTokenStore(); }
        else if (OperatingSystem.IsLinux()) { TokenStore.Instance = new LinuxTokenStore(); }

        return ab;
    }
}
