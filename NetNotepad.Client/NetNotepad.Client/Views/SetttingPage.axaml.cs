using Avalonia.Controls;
using NetNotepad.Client.ViewModels;

namespace NetNotepad.Client.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        DataContext = new SettingsPageModel();
        InitializeComponent();
    }
}