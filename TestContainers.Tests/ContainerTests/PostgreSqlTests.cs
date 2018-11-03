using Xunit;
using System.Threading.Tasks;
using Npgsql;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.GetConnectionString();
        private PostgreSqlContainer Container { get; }

        public PostgreSqlFixture() =>
             Container = new DatabaseContainerBuilder<PostgreSqlContainer>()
                .WithPassword("Password123")
                .Build();

        public Task InitializeAsync() => Container.StartAsync();

        public Task DisposeAsync() => Container.StopAsync();
    }

    public class PostgreSqlTests : IClassFixture<PostgreSqlFixture>
    {
        private readonly NpgsqlConnection _connection;

        public PostgreSqlTests(PostgreSqlFixture fixture) => _connection = new NpgsqlConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            const string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = await cmd.ExecuteScalarAsync();
            Assert.Equal(1, reader);

            _connection.Close();
        }
    }
}