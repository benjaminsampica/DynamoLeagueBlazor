using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "freeagents";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsNoPlayersWhoAreFreeAgents_ThenReturnsNothing()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(0);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWhoIsAFreeAgent_ThenReturnsOneFreeAgent()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;

        var biddingClosesOn = DateTime.MaxValue;
        mockPlayer.SetToFreeAgent(biddingClosesOn);
        await application.AddAsync(mockPlayer);

        var bidAmount = int.MaxValue;
        mockPlayer.AddBid(bidAmount, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(1);

        var freeAgent = result.FreeAgents.First();
        freeAgent.PlayerId.Should().Be(mockPlayer.Id);
        freeAgent.PlayerName.Should().Be(mockPlayer.Name);
        freeAgent.PlayerPosition.Should().Be(mockPlayer.Position);
        freeAgent.PlayerTeam.Should().Be(mockTeam.TeamName);
        freeAgent.PlayerHeadShotUrl.Should().Be(mockPlayer.HeadShot);
        freeAgent.HighestBid.Should().Be(bidAmount.ToString("C0"));
        freeAgent.BiddingEnds.Should().Be(biddingClosesOn.ToShortDateString());
        freeAgent.CurrentUserIsHighestBidder.Should().BeTrue();
    }
}
