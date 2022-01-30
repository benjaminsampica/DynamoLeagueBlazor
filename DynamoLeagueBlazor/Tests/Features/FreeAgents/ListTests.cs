using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class ListTests : IntegrationTestBase
{
    private const string _endpoint = "api/freeagents";

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsNoPlayersWhoAreFreeAgents_ThenReturnsNothing()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(0);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWhoIsAFreeAgent_ThenReturnsOneFreeAgent()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        mockPlayer.SetToRostered(DateTime.MinValue, int.MaxValue);
        var biddingEnds = DateTime.MaxValue;
        mockPlayer.SetToFreeAgent(biddingEnds);
        await application.AddAsync(mockPlayer);

        var bidAmount = int.MaxValue;
        mockPlayer.AddBid(bidAmount, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(1);

        var freeAgent = result.FreeAgents.First();
        freeAgent.Id.Should().Be(mockPlayer.Id);
        freeAgent.Name.Should().Be(mockPlayer.Name);
        freeAgent.Position.Should().Be(mockPlayer.Position);
        freeAgent.Team.Should().Be(mockTeam.Name);
        freeAgent.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        freeAgent.HighestBid.Should().Be(bidAmount.ToString("C0"));
        freeAgent.BiddingEnds.Should().Be(biddingEnds.ToShortDateString());
        freeAgent.CurrentUserIsHighestBidder.Should().BeTrue();
    }
}
