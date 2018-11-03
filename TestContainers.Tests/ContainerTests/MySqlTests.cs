using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class MySqlFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; }

        public MySqlFixture() =>
             Container = new DatabaseContainerBuilder<MySqlContainer>()
                .WithImage("mysql:5.7")
                .Build();

        public Task InitializeAsync() => Container.StartAsync();

        public Task DisposeAsync() => Container.StopAsync();
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        private readonly MySqlContainer _mySqlContainer;
        public MySqlTests(MySqlFixture fixture) => _mySqlContainer = fixture.Container;

        [Fact]
        public async Task SimpleTest()
        {
            using (var connection = new MySqlConnection(_mySqlContainer.GetConnectionString()))
            {
                await connection.OpenAsync();

                string query = "SELECT 1;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = await cmd.ExecuteScalarAsync();
                    Assert.Equal((long)1, reader);
                }
            }
        }
    }
}
