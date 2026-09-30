using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NetNotepad.Contracts;
using NetNotepad.ServiceBase;
using RabbitMQ.AMQP.Client;
using Serilog;
using StackExchange.Redis;

namespace NetNotepad.AuthService.ExternalWorker
{
    public class Program
    {
        public static JWTBuilder JWTBuilder { get; private set; } = new();
        public static JWTValidator JWTValidator { get; private set; } = new();
        public static IQueueSpecification HTTPAuthQueue { get; private set; } = null!;
        public static IQueueSpecification InternalAuthQueue { get; private set; } = null!;
        public static HookRouter<Responce> HTTPHookRouter { get; private set; } = new();

        public static IDatabase RedisDB { get; private set; } = null!;
        public static IConnection HTTPRabbitMQ { get; private set; } = null!;
        public static IConnection AuthRabbitMQ { get; private set; } = null!;
        public static RabbitMQEventHandler EventHandler { get; private set; } = null!;

        static async Task Main()
        {
            BloodLog.CreateLogger(typeof(Program).Namespace);

            Log.Information("Start application");
            string privatePem = File.ReadAllText("/run/secrets/jwt-secret-pri").Trim();
            string publicPem = File.ReadAllText("/run/secrets/jwt-secret-pub").Trim();
            if (!JWTKeyValidator.ValidatePems(privatePem, publicPem))
            {
                Log.Fatal("Keys are invalid");
                Environment.Exit(-1);
            }
            else { Log.Information("Keys validation succeful"); }
            JWTBuilder = JWTBuilder.SetPrivatePem(privatePem);
            JWTValidator = JWTValidator.SetPublicPem(publicPem);

            try
            {
                PostgresDBConnection.DBConnString = await PostgresClient.Connect();
                AppDBContext db = new();
                Log.Information("Current migrations count: {0}", db.Database.GetMigrations().Count());
                db.Database.Migrate();
                db.Dispose();
                Log.Information("Postgres connected");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Connection to postgres failed");
                Environment.Exit(-1);
            }

            try
            {
                RedisDB = await RedisClient.Connect();
                Log.Information("Redis connected");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Connection to redis failed");
                Environment.Exit(-1);
            }

            try
            {
                HTTPRabbitMQ = await RabbitMQClient.Connect("HTTP_RABBITMQ_HOST", "HTTP_RABBITMQ_PORT", "HTTP_RABBITMQ_USER", "HTTP_RABBITMQ_PASS");
                Log.Information("HTTP RabbitMQ connected");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Connection to http rabbitmq failed");
                Environment.Exit(-1);
            }

            try
            {
                EventHandler = new RabbitMQEventHandler(HTTPRabbitMQ, "AUTH_EVENT_NAME", events: [Events.USER_CREATED, Events.USER_LOGIN]);
                Log.Information("Created event handler");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create events handler");
                Environment.Exit(-1);
            }

            try
            {
                await CreateHTTPQueues();
                Log.Information("HTTP queues created");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create http queues");
                Environment.Exit(-1);
            }

            try
            {
                AuthRabbitMQ = await RabbitMQClient.Connect("AUTH_RABBITMQ_HOST", "AUTH_RABBITMQ_PORT", "AUTH_RABBITMQ_USER", "AUTH_RABBITMQ_PASS");
                Log.Information("Auth RabbitMQ connected");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Connection to auth rabbitmq failed");
                Environment.Exit(-1);
            }

            try
            {
                await CreateInternalQueues();
                Log.Information("Internal queues created");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create internal queues");
                Environment.Exit(-1);
            }

            Endpoints.AddEndpoints();
            await HTTPRabbitMQ.ResponderBuilder().RequestQueue(HTTPAuthQueue).Handler(HandleHTTPRequest).BuildAsync();
            await HTTPServer.Start(HandleEndpoint);
        }

        private static async Task CreateHTTPQueues()
        {
            IManagement management = HTTPRabbitMQ.Management();
            HTTPAuthQueue = management.Queue(Environment.GetEnvironmentVariable("AUTH_QUEUE_NAME") ?? throw new Exception("No auth queue name string"));

            IQueueSpecification[] queues = [HTTPAuthQueue];
            foreach (IQueueSpecification queue in queues) { await queue.Type(QueueType.QUORUM).DeclareAsync(); }
        }

        private static async Task CreateInternalQueues()
        {
            IManagement management = AuthRabbitMQ.Management();
            InternalAuthQueue = management.Queue(Environment.GetEnvironmentVariable("AUTH_INT_QUEUE_NAME") ?? throw new Exception("No auth internal queue name string"));

            IQueueSpecification[] queues = [InternalAuthQueue];
            foreach (IQueueSpecification queue in queues) { await queue.Type(QueueType.QUORUM).DeclareAsync(); }
        }

        private static async void HandleEndpoint(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            using HttpListenerResponse response = context.Response;
            string path = request.Url?.AbsolutePath.ToLower() ?? "";
            if (string.IsNullOrEmpty(path)) { return; }
            switch (path)
            {
                case "/health":
                    response.OutputStream.Write(Encoding.UTF8.GetBytes("OK"));
                    break;
                default:
                    Console.WriteLine($"Not found: {path}");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }
            context.Response.Close();
        }

        private static Task<IMessage> HandleHTTPRequest(IResponder.IContext ctx, IMessage message)
        {
            try
            {
                ServiceRequest sr = SerializeModule.Deserialize<ServiceRequest>(Encoding.UTF8.GetBytes(message.BodyAsString()));
                if (sr == null)
                {
                    Log.Error("Service request is invalid");
                    sr = new("", "");
                }
                if (HTTPHookRouter.Route(sr.Path, Encoding.UTF8.GetBytes(sr.Payload), out Responce? responce)) { }
                else { responce = new ServiceResponce(HttpStatusCode.NotFound, "NOT_FOUND"); }
                return Task.FromResult(ctx.Message(SerializeModule.Serialize(responce!)));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Exception while get request");
                return Task.FromResult(ctx.Message(SerializeModule.Serialize(new ServiceResponce(HttpStatusCode.InternalServerError, "INTERNAL_ERROR"))));
            }
        }
    }
}