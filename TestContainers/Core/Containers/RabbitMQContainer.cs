using System;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;

namespace TestContainers.Core.Containers
{
    public class RabbitMqContainer : GenericContainer
    {
        public const string Image = "rabbitmq";
        public const string DefaultTag = "3.7-alpine";
        public const int RabbitMqPort = 5672;
        private const int DefaultRequestedHeartbeatInSec = 60;

        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        public string RabbitMqUrl => $"amqp://{GetContainerIpAddress()}:{GetMappedPort(RabbitMqPort)}";
        
        public RabbitMqContainer(string tag) : base($"{Image}:{tag}") { }
        public RabbitMqContainer() : this(DefaultTag) { }

        protected override void Configure()
        {
            AddExposedPort(RabbitMqPort);
            AddEnv("RABBITMQ_DEFAULT_USER", UserName);
            AddEnv("RABBITMQ_DEFAULT_PASS", Password);
            AddEnv("RABBITMQ_DEFAULT_VHOST", VirtualHost);
        }

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();
            
            var connectionFactory = new ConnectionFactory
            {
                HostName = GetContainerIpAddress(),
                Port = GetMappedPort(RabbitMqPort),
                VirtualHost = VirtualHost,
                UserName = UserName,
                Password = Password,
                Protocol = Protocols.DefaultProtocol,
                RequestedHeartbeat = DefaultRequestedHeartbeatInSec
            };

            var result = Policy
                .Timeout(TimeSpan.FromMinutes(2))
                .Wrap(Policy
                    .Handle<Exception>()
                    .WaitAndRetryForever(iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCapture(() =>
                {
                    using (var connection = connectionFactory.CreateConnection())
                    {
                        if (!connection.IsOpen) throw new Exception("Connection not open");
                    }
                });

            if (result.Outcome == OutcomeType.Failure)
                throw new Exception(result.FinalException.Message);
        }
    }
}