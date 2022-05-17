using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class DetailTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var stubFreeAgent = CreateFakePlayer()
            .SetToRostered(DateTime.Today.AddYears(-1).Year, int.MaxValue)
            .SetToFreeAgent(DateTime.MaxValue);
        await application.AddAsync(stubFreeAgent);
        var endpoint = FreeAgentDetailFactory.Create(stubFreeAgent.Id);

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidPlayerId_ThenReturnsExpectedResult()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockFreeAgent = CreateFakePlayer()
            .SetToRostered(DateTime.Today.AddYears(-1).Year, int.MaxValue)
            .SetToFreeAgent(DateTime.MaxValue);
        mockFreeAgent.TeamId = mockTeam.Id;
        await application.AddAsync(mockFreeAgent);

        var bidAmount = int.MaxValue;
        mockFreeAgent.AddBid(bidAmount, mockTeam.Id);
        await application.UpdateAsync(mockFreeAgent);

        var client = application.CreateClient();
        var endpoint = FreeAgentDetailFactory.Create(mockFreeAgent.Id);

        var response = await client.GetFromJsonAsync<FreeAgentDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.Name.Should().Be(mockFreeAgent.Name);
        response.Position.Should().Be(mockFreeAgent.Position);
        response.Team.Should().Be(mockTeam.Name);
        response.HeadShotUrl.Should().Be(mockFreeAgent.HeadShotUrl);
        response.EndOfFreeAgency.Should().Be(mockFreeAgent.EndOfFreeAgency!.Value.ToShortDateString());

        response!.Bids.Should().HaveCount(1);
        var bid = response.Bids.First();
        bid.Team.Should().Be(mockTeam.Name);
        bid.Amount.Should().Be(bidAmount.ToString("C0"));
        DateTime.Parse(bid.CreatedOn).Should().BeExactly(TimeSpan.FromSeconds(0));
    }
}
