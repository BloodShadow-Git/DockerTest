using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;

namespace NetNotepad.Contracts
{
    public class RabbitMQEventHandler
    {
        private IConnection _connection = null!;
        private string _sourceName = null!;
        private string _targetName = null!;

        public RabbitMQEventHandler(IConnection connection, string targetNameVar, string sourceNameVar = "EVENT_NAME", params string[] events) => Create(connection, sourceNameVar, targetNameVar, events);

        private async void Create(IConnection connection, string sourceNameVar, string targetNameVar, params string[] events)
        {
            if (string.IsNullOrEmpty(sourceNameVar)) { throw new Exception("Empty source name"); }
            if (string.IsNullOrEmpty(targetNameVar)) { throw new Exception("Empty target name"); }
            if (events.Length <= 0) { throw new Exception("Empty event list"); }
            _sourceName = Environment.GetEnvironmentVariable(sourceNameVar) ?? throw new Exception("Empty source name variable");
            _targetName = Environment.GetEnvironmentVariable(targetNameVar) ?? throw new Exception("Empty source name variable");
            _connection = connection ?? throw new Exception("Empty connection object");

            IManagement management = _connection.Management();
            IExchangeSpecification exchange = management.Exchange(_sourceName).Type(ExchangeType.DIRECT);
            await exchange.DeclareAsync();
            IQueueSpecification queue = management.Queue(_targetName).Type(QueueType.QUORUM);
            await queue.DeclareAsync();

            foreach (string eventName in events)
            {
                IBindingSpecification binding = management
                    .Binding()
                    .SourceExchange(_sourceName)
                    .DestinationQueue(_targetName)
                    .Key(eventName);
                await binding.BindAsync();
            }
        }

        public async void Publish(string eventName, object message)
        {
            IPublisher publisher = await _connection.PublisherBuilder()
                .Exchange(_sourceName)
                .Key(eventName)
                .BuildAsync();

            await publisher.PublishAsync(new AmqpMessage(SerializeModule.Serialize(message)));
        }

        public async void Subscribe(MessageHandler handler)
        {
            await _connection.ConsumerBuilder()
                .Queue(_targetName)
                .MessageHandler(handler)
                .BuildAndStartAsync();
        }
    }
}