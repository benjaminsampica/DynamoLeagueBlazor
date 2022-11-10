using DynamoLeagueBlazor.Shared.Features.Dashboard;

namespace DynamoLeagueBlazor.Tests.Features.Dashboard;

public class TopOffendersTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(TopOffendersRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWithoutAFine_ThenReturnsZeroPlayers()
    {
        var application = GetUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        await AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(TopOffendersRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(0);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWithAFine_ThenReturnsOnePlayerWithAFine()
    {
        var application = GetUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        await AddAsync(mockPlayer);

        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        mockFine.Status = true;
        await UpdateAsync(mockFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(TopOffendersRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(1);

        var firstPlayer = result.Players.First();
        firstPlayer.Amount.Should().Be(mockFine.Amount.ToString("C0"));
        firstPlayer.Name.Should().Be(mockPlayer.Name);
        firstPlayer.ImageUrl.Should().Be(mockPlayer.HeadShotUrl);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsElevenPlayersWithApprovedFines_ThenReturnsOnlyTopTenByFineAmount()
    {
        var application = GetUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        foreach (var count in Enumerable.Range(0, 10))
        {
            var mockPlayer = CreateFakePlayer();
            mockPlayer.TeamId = stubTeam.Id;
            await AddAsync(mockPlayer);

            var fine = mockPlayer.AddFine(int.MaxValue, RandomString);
            fine.Status = true;
            await UpdateAsync(mockPlayer);
        }

        var eleventhPlayerWithFine = CreateFakePlayer();
        eleventhPlayerWithFine.TeamId = stubTeam.Id;
        await AddAsync(eleventhPlayerWithFine);

        var lowestFine = eleventhPlayerWithFine.AddFine(int.MinValue, RandomString);
        lowestFine.Status = true;
        await UpdateAsync(eleventhPlayerWithFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(TopOffendersRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(10);
        result.Players.Should().OnlyContain(p => p.Amount != lowestFine.Amount.ToString("C0"));
    }
}
