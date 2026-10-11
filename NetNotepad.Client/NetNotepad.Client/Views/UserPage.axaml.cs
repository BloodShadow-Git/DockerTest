using Avalonia.Controls;
using NetNotepad.Client.ViewModels;

namespace NetNotepad.Client.Views;

public partial class UserPage : ContentPage
{
    public UserPage()
    {
        DataContext = new UserPageModel();
        InitializeComponent();
    }
}