using DynamoLeagueBlazor.Shared.Features.Fines;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "fines";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_ThenDoesAllowAccess()
    {
        var application = CreateAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneFine_ThenReturnsOneFine()
    {
        var application = CreateAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var mockFine = CreateFakeFine();
        mockFine.PlayerId = mockPlayer.Id;
        await application.AddAsync(mockFine);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<GetFineListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Fines.Should().HaveCount(1);
    }
}
