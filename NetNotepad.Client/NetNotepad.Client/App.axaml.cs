using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetNotepad.Client.ViewModels;
using NetNotepad.Client.Views;

namespace NetNotepad.Client;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        { desktop.MainWindow = new MainWindow { DataContext = new LoginPageModel() }; }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        { singleViewFactoryApplicationLifetime.MainViewFactory = () => new LoginPage { DataContext = new LoginPageModel() }; }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        { singleViewPlatform.MainView = new LoginPage { DataContext = new LoginPageModel() }; }

        base.OnFrameworkInitializationCompleted();
    }
}