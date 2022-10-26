using DynamoLeagueBlazor.Shared.Features.Admin;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class StartSeasonTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsAFreeAgent_ThenReturnsTrue()
    {
        var application = GetAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.State = PlayerState.Rostered;
        mockPlayer.BeginNewSeason(DateTime.MaxValue);
        await AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(StartSeasonRouteFactory.Uri);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenNotPlayerIsAFreeAgent_ThenReturnsFalse()
    {
        var application = GetAdminAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(StartSeasonRouteFactory.Uri);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAFineExistsBeforeJanuary1stOfTheCurrentYear_ThenTheFineIsRemoved()
    {
        var application = GetAdminAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var stubPlayer = CreateFakePlayer();
        stubPlayer.State = PlayerState.Rostered;
        stubPlayer.TeamId = stubTeam.Id;
        var mockFine = stubPlayer.AddFine(int.MaxValue, RandomString);
        mockFine.CreatedOn = DateTime.MinValue;
        await AddAsync(stubPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await FirstOrDefaultAsync<Fine>();
        fine.Should().BeNull();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAFineExistsOnOrAfterJanuary1stOfTheCurrentYear_ThenTheFineIsRemoved()
    {
        var application = GetAdminAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var stubPlayer = CreateFakePlayer();
        stubPlayer.State = PlayerState.Rostered;
        stubPlayer.TeamId = stubTeam.Id;
        var mockFine = stubPlayer.AddFine(int.MaxValue, RandomString);
        mockFine.CreatedOn = new DateTime(DateTime.Today.Year, 1, 1);
        await AddAsync(stubPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsEligibleForFreeAgency_ThenSetsPlayerToFreeAgent()
    {
        var application = GetAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.YearContractExpires = null;
        await AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.EndOfFreeAgency.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsNotEligibleForFreeAgency_ThenSkipsThatPlayer()
    {
        var application = GetAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.YearContractExpires = int.MaxValue;
        mockPlayer.EndOfFreeAgency = null;
        await AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.EndOfFreeAgency.Should().BeNull();
    }
}
