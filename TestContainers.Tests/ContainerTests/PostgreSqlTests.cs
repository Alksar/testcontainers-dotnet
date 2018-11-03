using Xunit;
using System.Threading.Tasks;
using Npgsql;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }

        public PostgreSqlFixture() =>
             Container = new DatabaseContainerBuilder<PostgreSqlContainer>()
                .WithPassword("Password123")
                .Build();

        public Task InitializeAsync() => Container.StartAsync();

        public Task DisposeAsync() => Container.StopAsync();
    }

    public class PostgreSqlTests : IClassFixture<PostgreSqlFixture>
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;

        public PostgreSqlTests(PostgreSqlFixture fixture) => _postgreSqlContainer = fixture.Container;

        [Fact]
        public async Task SimpleTest()
        {
            using (var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString()))
            {
                await connection.OpenAsync();

                const string query = "SELECT 1;";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    var reader = await cmd.ExecuteScalarAsync();
                    Assert.Equal(1, reader);
                }
            }
        }
    }
}