using DynamoLeagueBlazor.Server.Models;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class StartSeasonTests : IntegrationTestBase
{
    private const string _endpoint = "admin/startseason";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(_endpoint, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(_endpoint, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsEligibleForFreeAgency_ThenSetsPlayerToFreeAgent()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.ContractLength = null;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(_endpoint, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await application.FindAsync<Player>(1);
        player!.EndOfFreeAgency.Should().NotBeNull();
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsNotEligibleForFreeAgency_ThenSkipsThatPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.ContractLength = int.MaxValue;
        mockPlayer.EndOfFreeAgency = null;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(_endpoint, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await application.FindAsync<Player>(1);
        player!.EndOfFreeAgency.Should().BeNull();
    }
}
