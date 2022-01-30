namespace DynamoLeagueBlazor.Tests;

[Collection("Server")]
public class IntegrationTestBase : IAsyncLifetime
{
    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync() => await ResetStateAsync();
}
