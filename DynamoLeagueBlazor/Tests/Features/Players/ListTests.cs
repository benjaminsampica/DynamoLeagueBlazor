using DynamoLeagueBlazor.Shared.Features.Players;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Players;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "api/players";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayer_ThenReturnsOnePlayer()
    {
        var application = CreateUserAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<PlayerListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(1);

        var player = result.Players.First();
        player.Name.Should().Be(mockPlayer.Name);
        player.Id.Should().Be(mockPlayer.Id);
        player.Position.Should().Be(mockPlayer.Position);
        player.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        player.Team.Should().Be(mockTeam.Name);
        player.YearContractExpires.Should().Be(mockPlayer.YearContractExpires.ToString());
        player.ContractValue.Should().Be(mockPlayer.ContractValue.ToString("C0"));
    }
}
