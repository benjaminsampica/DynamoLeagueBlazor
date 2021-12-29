namespace DynamoLeagueBlazor.Tests;

public class IntegrationTestBase
{
    [SetUp]
    public async Task SetUpBase()
    {
        await ResetStateAsync();
    }
}
