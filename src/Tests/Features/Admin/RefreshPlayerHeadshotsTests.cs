using DynamoLeagueBlazor.Shared.Features.Admin;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class RefreshPlayerHeadshotsTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_ThenUpdatesAPlayerHeadshot()
    {
        var application = CreateAdminAuthenticatedApplication();
        var player = CreateFakePlayer();
        var oldHeadshot = player.HeadShotUrl;
        await application.AddAsync(player);

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.Should().BeSuccessful();
        var expectedPlayerWithNewHeadshot = await application.FirstOrDefaultAsync<Player>();
        expectedPlayerWithNewHeadshot!.HeadShotUrl.Should().NotBe(oldHeadshot);
    }
}
