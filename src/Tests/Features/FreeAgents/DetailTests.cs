using DynamoLeagueBlazor.Client.Features.FreeAgents;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.Extensions.DependencyInjection;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class DetailServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();
        var client = application.CreateClient();

        var stubPlayer = CreateFakePlayer();
        await AddAsync(stubPlayer);
        var endpoint = FreeAgentDetailFactory.Create(stubPlayer.Id);

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidPlayerId_ThenReturnsExpectedResult()
    {
        var application = GetUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockFreeAgent = CreateFakePlayer();
        mockFreeAgent.TeamId = mockTeam.Id;
        await AddAsync(mockFreeAgent);

        mockFreeAgent.AddBid(Bid.MinimumAmount, mockTeam.Id);
        await UpdateAsync(mockFreeAgent);

        var client = application.CreateClient();
        var endpoint = FreeAgentDetailFactory.Create(mockFreeAgent.Id);

        var response = await client.GetFromJsonAsync<FreeAgentDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.Name.Should().Be(mockFreeAgent.Name);
        response.Position.Should().Be(mockFreeAgent.Position);
        response.Team.Should().Be(mockTeam.Name);
        response.HeadShotUrl.Should().Be(mockFreeAgent.HeadShotUrl);
        response.EndOfFreeAgency.Should().Be(mockFreeAgent.EndOfFreeAgency!.Value);

        response!.Bids.Should().HaveCount(1);
        var bid = response.Bids.First();
        bid.Team.Should().Be(mockTeam.Name);
        bid.Amount.Should().Be(Bid.MinimumAmount);
        DateTime.Parse(bid.CreatedOn).Should().BeExactly(TimeSpan.FromSeconds(0));
    }
}

public class DetailClientTests : UITestBase
{
    [Fact]
    public void WhenLoading_ThenShowsLoading()
    {
        TestContext!.Services.AddSingleton(Mock.Of<IBidValidator>());

        GetHttpHandler.When(HttpMethod.Get)
            .TimesOutAfter(5000);

        var cut = RenderComponent<Detail>();

        cut.HasComponent<MudSkeleton>().Should().BeTrue();
    }

    [Fact]
    public void GivenData_ThenShowsFormAndTimeline()
    {
        TestContext!.Services.AddSingleton(Mock.Of<IBidValidator>());

        GetHttpHandler.When(HttpMethod.Get)
            .RespondsWithJson(AutoFaker.Generate<FreeAgentDetailResult>());

        var cut = RenderComponent<Detail>();

        cut.HasComponent<MudTimeline>().Should().BeTrue();
        cut.HasComponent<MudNumericField<int>>().Should().BeTrue();
    }
}
