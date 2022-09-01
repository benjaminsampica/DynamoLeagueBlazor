using DynamoLeagueBlazor.Shared.Features.Admin;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class RefreshPlayerHeadshotsTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_ThenUpdatesAPlayerHeadshot()
    {
        var application = GetAdminAuthenticatedApplication();
        var player = CreateFakePlayer();
        var oldHeadshot = player.HeadShotUrl;
        await AddAsync(player);

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.Should().BeSuccessful();
        var expectedPlayerWithNewHeadshot = await FirstOrDefaultAsync<Player>();
        expectedPlayerWithNewHeadshot!.HeadShotUrl.Should().NotBe(oldHeadshot);
    }
}
