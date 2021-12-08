namespace DynamoLeagueBlazor.Tests;

public class IntegrationTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await ResetStateAsync();
    }
}
