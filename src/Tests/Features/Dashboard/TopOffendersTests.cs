using DynamoLeagueBlazor.Shared.Features.Dashboard;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Dashboard;

public class TopOffendersTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(TopOffendersRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWithoutAFine_ThenReturnsZeroPlayers()
    {
        var application = CreateUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(TopOffendersRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(0);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWithAFine_ThenReturnsOnePlayerWithAFine()
    {
        var application = CreateUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        mockFine.Status = true;
        await application.AddAsync(mockPlayer);

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
        var application = CreateUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        foreach (var count in Enumerable.Range(0, 10))
        {
            var mockPlayer = CreateFakePlayer();
            mockPlayer.TeamId = stubTeam.Id;
            var fine = mockPlayer.AddFine(int.MaxValue, RandomString);
            fine.Status = true;
            await application.AddAsync(mockPlayer);
        }

        var eleventhPlayerWithFine = CreateFakePlayer();
        eleventhPlayerWithFine.TeamId = stubTeam.Id;
        var lowestFine = eleventhPlayerWithFine.AddFine(int.MinValue, RandomString);
        lowestFine.Status = true;
        await application.AddAsync(eleventhPlayerWithFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(TopOffendersRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(10);
        result.Players.Should().OnlyContain(p => p.Amount != lowestFine.Amount.ToString("C0"));
    }
}
