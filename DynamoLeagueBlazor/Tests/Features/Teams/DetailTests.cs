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

        var mockRosteredPlayer = CreateFakePlayer();
        mockRosteredPlayer.TeamId = stubTeam.Id;
        mockRosteredPlayer.SetToRostered(DateTime.MaxValue, 1);
        await application.AddAsync(mockRosteredPlayer);

        var mockUnrosteredPlayer = CreateFakePlayer();
        mockUnrosteredPlayer.TeamId = stubTeam.Id;
        mockUnrosteredPlayer.SetToUnrostered();
        await application.AddAsync(mockUnrosteredPlayer);

        var mockUnsignedPlayer = CreateFakePlayer();
        mockUnsignedPlayer.TeamId = stubTeam.Id;
        mockUnsignedPlayer.SetToUnsigned();
        await application.AddAsync(mockUnsignedPlayer);

        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetFromJsonAsync<TeamDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.TeamName.Should().Be(stubTeam.TeamName);
        response.CapSpace.Should().Be((mockRosteredPlayer.ContractValue + mockUnrosteredPlayer.ContractValue / 2).ToString("C0"));
        response.RosteredPlayers.Should().NotBeEmpty();
        response.UnrosteredPlayers.Should().NotBeEmpty();
        response.UnsignedPlayers.Should().NotBeEmpty();
    }
}
