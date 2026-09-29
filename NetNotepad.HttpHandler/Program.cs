using System.Net;
using System.Text;
using NetNotepad.Contracts;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using Serilog;
using StackExchange.Redis;
using NetNotepad.ServiceBase;

namespace NetNotepad.HttpHandler
{
    public class Program
    {
        public static IPublisher Publisher { get; private set; } = null!;

        public static IQueueSpecification AuthQueue { get; private set; } = null!;
        public static IQueueSpecification UserQueue { get; private set; } = null!;
        public static IQueueSpecification NotepadQueue { get; private set; } = null!;

        public static IDatabase RedisDB { get; private set; } = null!;
        public static IConnection RabbitMQ { get; private set; } = null!;

        static async Task Main()
        {
            BloodLog.CreateLogger(typeof(Program).Namespace);

            Log.Information("Start application");

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
                RabbitMQ = await RabbitMQClient.Connect();
                Log.Information("RabbitMQ connected");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Connection to rabbitMQ failed");
                Environment.Exit(-1);
            }

            try
            {
                await CreateQueues();
                Log.Information("Queues created");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create queues");
                Environment.Exit(-1);
            }

            try
            {
                await CreatePublisher();
                Log.Information("Publisher created");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create publisher");
                Environment.Exit(-1);
            }

            await HTTPServer.Start(HandleEndpoint);
        }

        private static async Task CreatePublisher() { Publisher = await RabbitMQ.PublisherBuilder().BuildAsync(); }

        private static async Task CreateQueues()
        {
            IManagement management = RabbitMQ.Management();
            AuthQueue = management.Queue(Environment.GetEnvironmentVariable("AUTH_QUEUE_NAME") ?? throw new Exception("No auth queue name string"));
            UserQueue = management.Queue(Environment.GetEnvironmentVariable("USER_QUEUE_NAME") ?? throw new Exception("No user queue name string"));
            NotepadQueue = management.Queue(Environment.GetEnvironmentVariable("NOTEPAD_QUEUE_NAME") ?? throw new Exception("No notepad queue name string"));

            IQueueSpecification[] queues = [AuthQueue, UserQueue, NotepadQueue];
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
                    if (path.StartsWith("/auth")) { await Send(AuthQueue, context, path); }
                    else if (path.StartsWith("/user")) { await Send(UserQueue, context, path); }
                    else if (path.StartsWith("/notepad")) { await Send(NotepadQueue, context, path); }
                    else
                    {
                        Log.Information($"Not found: {path}");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    }
                    response.StatusCode = (int)HttpStatusCode.OK;
                    break;
            }
            context.Response.Close();
        }

        private static async Task Send(IQueueSpecification queue, HttpListenerContext context, string path)
        {
            IRequester requester = await RabbitMQ.RequesterBuilder().RequestAddress().Queue(queue).Requester().BuildAsync();
            IMessage message = await requester.PublishAsync(new AmqpMessage(SerializeModule.Serialize(
                new ServiceRequest(path, new StreamReader(context.Request.InputStream).ReadToEnd())
            )));
            await context.Response.OutputStream.WriteAsync(context.Request.ContentEncoding.GetBytes(message.BodyAsString()));
        }
    }
}