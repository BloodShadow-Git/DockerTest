using Serilog;

namespace NetNotepad.ServiceBase
{
    public static class BloodLog
    {
        public static void CreateLogger(string? name)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty($"Service-{name}", $"{name}_{Guid.NewGuid()}")
#if DEBUG
                .WriteTo.Console()
#else
                .WriteTo.Console(new RenderedCompactJsonFormatter())
#endif
                .CreateLogger();
        }
    }
}