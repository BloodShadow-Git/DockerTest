using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetNotepad.Client.Source;
using NetNotepad.Client.ViewModels;
using NetNotepad.Client.Views;

namespace NetNotepad.Client;

public partial class App : Application
{
    public override void Initialize()
    {
        Settings.Init();
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        { desktop.MainWindow = new MainWindow { DataContext = new NewLoginPageModel() }; }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        { singleViewFactoryApplicationLifetime.MainViewFactory = () => new NewLoginPage { DataContext = new NewLoginPageModel() }; }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        { singleViewPlatform.MainView = new NewLoginPage { DataContext = new NewLoginPageModel() }; }

        base.OnFrameworkInitializationCompleted();
    }
}