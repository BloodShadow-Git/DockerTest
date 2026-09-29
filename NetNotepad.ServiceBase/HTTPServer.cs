using System.Net;
using NetNotepad.Contracts;
using Serilog;
using Serilog.Formatting.Compact;

namespace NetNotepad.ServiceBase
{
    public class HTTPServer
    {
        public static CancellationToken StopToken => _tokenSource.Token;
        private static CancellationTokenSource _tokenSource = new();
        private static HttpListener _listener = new();
        public static ushort PORT => _port;
        private static ushort _port = 8080;
        private static Action<object?, EventArgs>? _onExit;

        public static async Task Start(Action<HttpListenerContext> onHandle, Action<object?, EventArgs>? onExit = null)
        {
            _onExit = onExit;

            string? portString = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(portString))
            {
                Log.Information("Found \"PORT\" key");
                if (!ushort.TryParse(portString, out _port))
                {
                    _port = 8080;
                    Log.Error($"Invalid key: {portString}/ Use default port: 8080");
                }
                Log.Information($"Set \"PORT\" to {_port}");
            }

            _listener.Prefixes.Clear();
            _listener.Prefixes.Add($"http://*:{_port}/");
            try
            {
                _listener.Start();
                Log.Information($"Run at {_port}");
            }
            catch (Exception ex)
            {
                Log.Fatal($"Critical error:\n{ex}");
                Environment.Exit(-1);
            }

            while (!StopToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    _ = Task.Run(() =>
                    {
                        if (!_listener.IsListening)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                            return;
                        }
                        HttpListenerRequest request = context.Request;
                        Log.Information("Getting request from {0} with id \"{1}\" {2} {3}",
                                        request.Headers[HTTPHeaders.X_Real_IP], request.Headers[HTTPHeaders.X_Request_ID],
                                        request.HttpMethod, request.Url?.AbsolutePath.ToLower() ?? "");
                        onHandle?.Invoke(context);
                    });
                }
                catch { }
            }
        }

        public static void Stop()
        {
            Log.Information("Stopping application");
            _tokenSource.Cancel();
            _listener.Stop();
            _listener.Close();
            Log.Information("Application stopped");
        }
    }
}