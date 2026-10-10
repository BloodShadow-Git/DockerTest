using Avalonia.Controls;
using NetNotepad.Client.ViewModels;

namespace NetNotepad.Client.Views;

public partial class AccountSelectPage : ContentPage
{
    public AccountSelectPage()
    {
        DataContext = new AccountSelectPageModel();
        InitializeComponent();
    }
}