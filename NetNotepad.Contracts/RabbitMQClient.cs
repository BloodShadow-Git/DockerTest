using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using Serilog;

namespace NetNotepad.Contracts
{
    public static class RabbitMQClient
    {

        public static Task<IConnection> Connect(string hostVar = "RABBITMQ_HOST", string portVar = "RABBITMQ_PORT",
            string userVar = "RABBITMQ_USER", string passVar = "RABBITMQ_PASS")
        {
            string rabbitHost = Environment.GetEnvironmentVariable(hostVar) ?? throw new Exception("No RabbitMQ host string");
            int rabbitPort = int.Parse(Environment.GetEnvironmentVariable(portVar) ?? throw new Exception("No RabbitMQ port string"));
            string rabbitUser = Environment.GetEnvironmentVariable(userVar) ?? throw new Exception("No RabbitMQ user string");
            string rabbitPass = Environment.GetEnvironmentVariable(passVar) ?? throw new Exception("No RabbitMQ password string");
            Log.Information("Found rabbitMQ host, port, user, password strings");
            IEnvironment environment = AmqpEnvironment.Create(new ConnectionSettings("amqp", rabbitHost, rabbitPort, rabbitUser, rabbitPass));
            return environment.CreateConnectionAsync();
        }
    }
}