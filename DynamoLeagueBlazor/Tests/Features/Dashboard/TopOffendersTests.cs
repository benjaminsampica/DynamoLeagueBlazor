using DynamoLeagueBlazor.Shared.Features.Dashboard;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Dashboard;

public class TopOffendersTests : IntegrationTestBase
{
    private const string _endpoint = "api/dashboard/topoffenders";

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
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
        firstPlayer.TotalFineAmount.Should().Be(mockFine.Amount.ToString("C0"));
        firstPlayer.Name.Should().Be(mockPlayer.Name);
        firstPlayer.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsElevenPlayersWithApprovedFines_ThenReturnsOnlyTopTenByFineAmount()
    {
        var application = CreateUserAuthenticatedApplication();
        foreach (var count in Enumerable.Range(0, 10))
        {
            var mockPlayer = CreateFakePlayer();
            await application.AddAsync(mockPlayer);

            var mockFine = CreateFakeFine(mockPlayer.Id);
            mockFine.Status = true;
            mockFine.Amount = int.MaxValue;
            await application.AddAsync(mockFine);
        }

        var sixthPlayer = CreateFakePlayer();
        await application.AddAsync(sixthPlayer);

        var lowestFine = CreateFakeFine(sixthPlayer.Id);
        lowestFine.Status = true;
        lowestFine.Amount = 1;
        await application.AddAsync(lowestFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TopOffendersResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(10);
        result.Players.Should().OnlyContain(p => p.TotalFineAmount != lowestFine.Amount.ToString("C0"));
    }
}
