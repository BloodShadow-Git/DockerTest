using System;
using Avalonia;

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

        ab.WithInterFont()
          .LogToTrace();
        return ab;
    }
}
