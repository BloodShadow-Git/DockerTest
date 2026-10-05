using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace NetNotepad.Client.Views;

public partial class MainWindow : Window
{
    public static WindowNotificationManager NM { get; private set; } = null!;

    public MainWindow()
    {
        InitializeComponent();
        NM = NotificationManager;
    }
}