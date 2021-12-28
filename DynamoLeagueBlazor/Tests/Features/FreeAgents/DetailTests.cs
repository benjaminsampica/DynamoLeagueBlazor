using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

internal class DetailTests : IntegrationTestBase
{
    private const string _endpoint = "/freeagents/";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var stubFreeAgent = CreateFakePlayer()
            .SetToRostered(DateTime.Today.AddYears(-1), int.MaxValue)
            .SetToFreeAgent(DateTime.MaxValue);
        await application.AddAsync(stubFreeAgent);
        var endpoint = _endpoint + stubFreeAgent.Id;

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidPlayerId_ThenReturnsExpectedResult()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockFreeAgent = CreateFakePlayer()
            .SetToRostered(DateTime.Today.AddYears(-1), int.MaxValue)
            .SetToFreeAgent(DateTime.MaxValue);
        mockFreeAgent.TeamId = mockTeam.Id;
        await application.AddAsync(mockFreeAgent);

        var bidAmount = int.MaxValue;
        mockFreeAgent.AddBid(bidAmount, mockTeam.Id);
        await application.UpdateAsync(mockFreeAgent);

        var client = application.CreateClient();
        var endpoint = _endpoint + mockFreeAgent.Id;

        var response = await client.GetFromJsonAsync<FreeAgentDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.Name.Should().Be(mockFreeAgent.Name);
        response.Position.Should().Be(mockFreeAgent.Position);
        response.Team.Should().Be(mockTeam.Name);
        response.HeadShot.Should().Be(mockFreeAgent.HeadShotUrl);
        response.EndOfFreeAgency.Should().Be(mockFreeAgent.EndOfFreeAgency!.Value.ToShortDateString());

        response!.Bids.Should().HaveCount(1);
        var bid = response.Bids.First();
        bid.Team.Should().Be(mockTeam.Name);
        bid.Amount.Should().Be(bidAmount.ToString("C0"));
        DateTime.Parse(bid.CreatedOn).Should().BeExactly(TimeSpan.FromSeconds(0));
    }
}
