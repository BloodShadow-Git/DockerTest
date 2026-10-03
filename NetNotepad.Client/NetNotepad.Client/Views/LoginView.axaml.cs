using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NetNotepad.Client.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        System.Diagnostics.Debug.WriteLine($"DataContext: {DataContext?.GetType().Name}");
    }
}