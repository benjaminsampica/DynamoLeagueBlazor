using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class DropPlayerServerTests : IntegrationTestBase
{
    private static DropPlayerRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<DropPlayerRequest>();
        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(DropPlayerRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_WhenAPlayerIsRostered_ThenUnrostersPlayer()
    {
        var application = GetUserAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.Rostered;
        await AddAsync(mockPlayer);

        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var client = application.CreateClient();

        await client.PostAsJsonAsync(DropPlayerRouteFactory.Uri, request);

        var player = await FirstOrDefaultAsync<Player>();

        player!.State.Should().Be(PlayerState.Unrostered);
    }
}