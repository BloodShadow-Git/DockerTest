using Avalonia.Controls;
using NetNotepad.Client.ViewModels;

namespace NetNotepad.Client.Views;

public partial class NewLoginPage : ContentPage
{
    public NewLoginPage()
    {
        DataContext = new NewLoginPageModel();
        InitializeComponent();
    }
}