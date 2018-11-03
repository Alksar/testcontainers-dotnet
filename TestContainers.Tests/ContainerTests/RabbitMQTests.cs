using System.Threading.Tasks;
using Xunit;
using RabbitMQ.Client;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;

namespace TestContainers.Tests.ContainerTests
{
    public class RabbitMqFixture : IAsyncLifetime
    {
        public RabbitMqContainer Container { get; }
    
        public RabbitMqFixture() =>
            Container = new RabbitMqContainerBuilder()
                .WithUser("admin")
                .WithPassword("admin")
                .Build();
    
        public Task InitializeAsync() => Container.StartAsync();
    
        public Task DisposeAsync() => Container.StopAsync();
    }
    
    public class RabbitMqTests : IClassFixture<RabbitMqFixture>
    {
        private readonly RabbitMqContainer _rabbitMqContainer;
    
        public RabbitMqTests(RabbitMqFixture fixture) => _rabbitMqContainer = fixture.Container;
        
        [Fact]
        public void OpenModelTest()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _rabbitMqContainer.GetContainerIpAddress(),
                Port = _rabbitMqContainer.GetMappedPort(RabbitMqContainer.RabbitMqPort),
                VirtualHost = _rabbitMqContainer.VirtualHost,
                UserName = _rabbitMqContainer.UserName,
                Password = _rabbitMqContainer.Password,
                Protocol = Protocols.DefaultProtocol
            };

            using (var connection = connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                Assert.True(model.IsOpen);
            }
        }
    }
}