using DynamoLeagueBlazor.Client.Features.Teams;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class DetailServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();
        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var client = application.CreateClient();
        var endpoint = TeamDetailRouteFactory.Create(stubTeam.Id);

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidTeamId_ThenReturnsExpectedResult()
    {
        var application = GetUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockRosteredPlayer = CreateFakePlayer();
        mockRosteredPlayer.TeamId = stubTeam.Id;
        mockRosteredPlayer.SignForCurrentTeam(DateTimeOffset.MaxValue.Year, 1);
        mockRosteredPlayer.State = PlayerState.Rostered;
        await AddAsync(mockRosteredPlayer);

        var mockUnrosteredPlayer = CreateFakePlayer();
        mockUnrosteredPlayer.TeamId = stubTeam.Id;
        mockUnrosteredPlayer.SetToUnrostered();
        mockUnrosteredPlayer.State = PlayerState.Unrostered;
        await AddAsync(mockUnrosteredPlayer);

        var mockUnsignedPlayer = CreateFakePlayer();
        mockUnsignedPlayer.TeamId = stubTeam.Id;
        mockUnsignedPlayer.State = PlayerState.Unsigned;
        await AddAsync(mockUnsignedPlayer);

        var client = application.CreateClient();
        var endpoint = TeamDetailRouteFactory.Create(stubTeam.Id);

        var response = await client.GetFromJsonAsync<TeamDetailResult>(endpoint);
        response.Should().NotBeNull();
        response!.Name.Should().Be(stubTeam.Name);
        var expectedCapSpace = CapSpaceUtilities.GetRemainingCapSpace(
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime),
            mockRosteredPlayer.ContractValue,
            mockUnrosteredPlayer.ContractValue,
            mockUnsignedPlayer.ContractValue).ToString("C0");
        response.CapSpace.Should().Be(expectedCapSpace);

        response.RosteredPlayers.Should().NotBeEmpty();
        var rosteredPlayer = response.RosteredPlayers.First();
        rosteredPlayer.HeadShotUrl.Should().Be(mockRosteredPlayer.HeadShotUrl);
        rosteredPlayer.Name.Should().Be(mockRosteredPlayer.Name);
        rosteredPlayer.Position.Should().Be(mockRosteredPlayer.Position);
        rosteredPlayer.YearContractExpires.Should().Be(mockRosteredPlayer.YearContractExpires);
        rosteredPlayer.ContractValue.Should().Be(mockRosteredPlayer.ContractValue);

        response.UnrosteredPlayers.Should().NotBeEmpty();
        var unrosteredPlayer = response.UnrosteredPlayers.First();
        unrosteredPlayer.HeadShotUrl.Should().Be(mockUnrosteredPlayer.HeadShotUrl);
        unrosteredPlayer.Name.Should().Be(mockUnrosteredPlayer.Name);
        unrosteredPlayer.Position.Should().Be(mockUnrosteredPlayer.Position);
        unrosteredPlayer.YearContractExpires.Should().Be(mockUnrosteredPlayer.YearContractExpires);
        unrosteredPlayer.ContractValue.Should().Be(mockUnrosteredPlayer.ContractValue);

        response.UnsignedPlayers.Should().NotBeEmpty();
        var unsignedPlayer = response.UnsignedPlayers.First();
        unsignedPlayer.HeadShotUrl.Should().Be(mockUnsignedPlayer.HeadShotUrl);
        unsignedPlayer.Name.Should().Be(mockUnsignedPlayer.Name);
        unsignedPlayer.Position.Should().Be(mockUnsignedPlayer.Position);
        unsignedPlayer.YearContractExpires.Should().Be(mockUnsignedPlayer.YearContractExpires);
        unsignedPlayer.ContractValue.Should().Be(mockUnsignedPlayer.ContractValue);
    }
}

public class DetailUITests : UITestBase
{
    [Fact]
    public void WhenPageIsLoading_ThenShowsLoading()
    {
        AuthorizeAsUser(int.MaxValue);

        GetHttpHandler.When(HttpMethod.Get, TeamDetailRouteFactory.Create(int.MaxValue))
            .Respond(with => with.ClientTimeout(TimeSpan.FromSeconds(5)));

        var cut = RenderComponent<Detail>(parameters =>
        {
            parameters.Add(p => p.TeamId, int.MaxValue);
        });

        cut.HasComponent<MudSkeleton>().Should().BeTrue();
    }

    [Fact]
    public void WhenThePageInitializes_ThenShowsAListOfRosteredPlayers()
    {
        var teamId = int.MaxValue;

        AuthorizeAsUser(teamId);

        GetHttpHandler.When(HttpMethod.Get, TeamDetailRouteFactory.Create(teamId))
            .RespondsWithJson(AutoFaker.Generate<TeamDetailResult>());

        var cut = RenderComponent<Detail>(parameters =>
        {
            parameters.Add(p => p.TeamId, teamId);
        });

        cut.Markup.Should().Contain("Rostered Players");
        cut.HasComponent<MudTable<TeamDetailResult.PlayerItem>>().Should().BeTrue();
        cut.Markup.Should().Contain("tr");
    }

    [Fact]
    public void GivenAUserHasATeamOfOne_WhenViewingThePageOfTeamTwo_ThenDoesNotShowAnyActionButtons()
    {
        AuthorizeAsUser(1);

        var teamTwo = 2;
        GetHttpHandler.When(HttpMethod.Get, TeamDetailRouteFactory.Create(teamTwo))
            .RespondsWithJson(AutoFaker.Generate<TeamDetailResult>());

        var cut = RenderComponent<Detail>(parameters =>
        {
            parameters.Add(p => p.TeamId, teamTwo);
        });

        cut.Markup.Should().NotContain("Actions");
    }

    [Fact]
    public void GivenAnAdmin_ThenCanAddAFine()
    {
        var teamId = int.MaxValue;
        AuthorizeAsAdmin(teamId);

        GetHttpHandler.When(HttpMethod.Get, TeamDetailRouteFactory.Create(teamId))
            .RespondsWithJson(AutoFaker.Generate<TeamDetailResult>());

        var cut = RenderComponent<Detail>(parameters =>
        {
            parameters.Add(p => p.TeamId, teamId);
        });

        cut.Markup.Should().Contain("Fine This Team");
    }
}