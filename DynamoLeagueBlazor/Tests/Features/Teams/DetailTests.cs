using DynamoLeagueBlazor.Shared.Features.Teams;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

internal class DetailTests : IntegrationTestBase
{
    private const string _endpoint = "/teams/";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);
        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_ThenDoesAllowAccess()
    {
        var application = CreateAuthenticatedApplication();
        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);
        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidTeamId_ThenReturnsExpectedResult()
    {
        var application = CreateAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        var stubPlayer = CreateFakePlayer();
        stubPlayer.TeamId = stubTeam.Id;
        await application.AddAsync(stubPlayer);

        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetFromJsonAsync<TeamDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.TeamName.Should().Be(stubTeam.TeamName);
        response.CapSpace.Should().Be(stubPlayer.ContractValue.ToString("C0"));
        response.RosteredPlayers.Should().NotBeEmpty();
        response.UnrosteredPlayers.Should().NotBeEmpty();
    }
}
