using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace DynamoLeagueBlazor.Shared.Infrastructure;

public class MsSqlContainerFactory : IAsyncDisposable
{
    private static MsSqlTestcontainer _msSqlTestContainer = null!;

    private MsSqlContainerFactory() { }

    public static async Task<string> CreateAsync()
    {
        _msSqlTestContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Database = "DynamoLeagueDb",
                Password = "yourStrong!Password123"
            })
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .Build();

        await _msSqlTestContainer.StartAsync();

        var connectionString = _msSqlTestContainer.ConnectionString + "TrustServerCertificate=true"; // Local development can ignore ssl certificates.

        return connectionString;
    }

    public ValueTask DisposeAsync() => _msSqlTestContainer.DisposeAsync();
}
