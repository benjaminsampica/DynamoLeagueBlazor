using DynamoLeagueBlazor.Shared.Features.Dashboard;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Dashboard;

internal class TopOffendersTests : IntegrationTestBase
{
    private const string _endpoint = "dashboard/topoffenders";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerWithAFine_ThenReturnsOnePlayerWithAFine()
    {
        var application = CreateUserAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var mockFine = CreateFakeFine(mockPlayer.Id);
        mockFine.Status = true;
        await application.AddAsync(mockFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(1);

        var firstPlayer = result.Players.First();
        firstPlayer.TotalFineAmount.Should().Be(mockFine.FineAmount.ToString("C0"));
        firstPlayer.PlayerName.Should().Be(mockPlayer.Name);
        firstPlayer.PlayerHeadShotUrl.Should().Be(mockPlayer.HeadShot);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsElevenPlayersWithApprovedFines_ThenReturnsOnlyTopTenByFineAmount()
    {
        var application = CreateUserAuthenticatedApplication();
        foreach (var count in Enumerable.Range(0, 10))
        {
            var mockPlayer = CreateFakePlayer();
            await application.AddAsync(mockPlayer);

            var mockFine = CreateFakeFine(mockPlayer.Id);
            mockFine.Status = true;
            mockFine.FineAmount = int.MaxValue;
            await application.AddAsync(mockFine);
        }

        var sixthPlayer = CreateFakePlayer();
        await application.AddAsync(sixthPlayer);

        var lowestFine = CreateFakeFine(sixthPlayer.Id);
        lowestFine.Status = true;
        lowestFine.FineAmount = 1;
        await application.AddAsync(lowestFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(10);
        result.Players.Should().OnlyContain(p => p.TotalFineAmount != lowestFine.FineAmount.ToString("C0"));
    }
}
