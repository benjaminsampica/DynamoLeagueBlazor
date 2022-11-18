using DynamoLeagueBlazor.Shared.Features.FreeAgents;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class ListTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(FreeAgentListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsNoPlayersWhoAreFreeAgents_ThenReturnsNothing()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(FreeAgentListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(0);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWhoIsAFreeAgent_ThenReturnsOneFreeAgent()
    {
        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        mockPlayer.SignForCurrentTeam(DateTime.MinValue.Year, int.MaxValue);
        var biddingEnds = DateTime.MaxValue;
        mockPlayer.BeginNewSeason(biddingEnds);
        await AddAsync(mockPlayer);

        mockPlayer.AddBid(Bid.MinimumAmount, mockTeam.Id);
        await UpdateAsync(mockPlayer);

        var application = GetUserAuthenticatedApplication(mockTeam.Id);
        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(FreeAgentListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.FreeAgents.Should().HaveCount(1);

        var freeAgent = result.FreeAgents.First();
        freeAgent.Id.Should().Be(mockPlayer.Id);
        freeAgent.Name.Should().Be(mockPlayer.Name);
        freeAgent.Position.Should().Be(mockPlayer.Position);
        freeAgent.Team.Should().Be(mockTeam.Name);
        freeAgent.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        freeAgent.HighestBid.Should().Be(Bid.MinimumAmount);
        freeAgent.BiddingEnds.Should().Be(biddingEnds);
        freeAgent.CurrentUserIsHighestBidder.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenAFreeAgentHasABidByAnotherTeam_ThenThatTeamShowsAsTheWinningTeam()
    {
        var application = GetUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        mockPlayer.SignForCurrentTeam(DateTime.MinValue.Year, int.MaxValue);
        var biddingEnds = DateTime.MaxValue;
        mockPlayer.BeginNewSeason(biddingEnds);
        await AddAsync(mockPlayer);

        var winningTeam = CreateFakeTeam();
        await AddAsync(winningTeam);

        var bidAmount = int.MaxValue;
        mockPlayer.AddBid(bidAmount, winningTeam.Id);
        await UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FreeAgentListResult>(FreeAgentListRouteFactory.Uri);

        var freeAgent = result!.FreeAgents.First();
        freeAgent.WinningTeam.Should().Be(winningTeam.Name);
    }
}
