using DynamoLeagueBlazor.Shared.Features.Fines;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class ListTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(FineListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneFine_ThenReturnsOneFine()
    {
        var application = CreateUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FineListResult>(FineListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Fines.Should().HaveCount(1);

        var fine = result!.Fines.First();
        fine.Id.Should().Be(mockFine.Id);
        fine.PlayerHeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        fine.PlayerName.Should().Be(mockPlayer.Name);
        fine.Status.Should().BeOneOf("Pending", "Approved");
        fine.Amount.Should().Be(mockFine.Amount);
        fine.Reason.Should().Be(mockFine.Reason);
        fine.TeamName.Should().Be(mockTeam.Name);
        fine.TeamLogoUrl.Should().Be(mockTeam.LogoUrl);
    }
}
