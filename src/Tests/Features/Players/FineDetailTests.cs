using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Features.Players;

public class FineDetailTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = FineDetailRouteFactory.Create(int.MaxValue);

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerIsFound_ThenReturnsExpectedResult()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var player = CreateFakePlayer();
        await application.AddAsync(player);

        var endpoint = FineDetailRouteFactory.Create(player.Id);

        var response = await client.GetFromJsonAsync<FineDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.FineAmount.Should().Be(FineUtilities.CalculateFineAmount(player.ContractValue).ToString("C0"));
        response.ContractValue.Should().Be(player.ContractValue.ToString("C0"));
    }
}
