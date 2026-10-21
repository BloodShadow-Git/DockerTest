using Avalonia.Controls.Notifications;
using NetNotepad.Client.Views;
using NetNotepad.Contracts;

namespace Extensions
{
    public static partial class Extensions
    {
        public static void Show(this Response resp, NotificationType type) { MainWindow.NM.Show(new Notification(resp.Message, null, type)); }
    }
}