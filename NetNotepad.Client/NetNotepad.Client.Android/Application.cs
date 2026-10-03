using System.Diagnostics;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using NetNotepad.Client.Source;

namespace NetNotepad.Client.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            TokenStore.Instance = new AndroidTokenStore();
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
            .UseR3(ex => Debug.WriteLine(ex))
            .WithInterFont();
        }
    }
}
