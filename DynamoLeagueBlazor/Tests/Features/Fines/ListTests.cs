using DynamoLeagueBlazor.Shared.Features.Fines;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "api/fines";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneFine_ThenReturnsOneFine()
    {
        var application = CreateUserAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var mockFine = mockPlayer.AddFine(1, RandomString);
        await application.UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FineListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Fines.Should().HaveCount(1);

        var fine = result!.Fines.First();
        fine.Id.Should().Be(mockFine.Id);
        fine.PlayerHeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        fine.PlayerName.Should().Be(mockPlayer.Name);
        fine.Status.Should().BeOneOf("Pending", "Approved");
        fine.Amount.Should().Be(mockFine.Amount.ToString("C2"));
        fine.Reason.Should().Be(mockFine.Reason);
    }
}
