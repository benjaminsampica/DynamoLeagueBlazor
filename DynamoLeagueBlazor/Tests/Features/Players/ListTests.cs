using DynamoLeagueBlazor.Shared.Features.Players;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Players;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "players";

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
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayer_ThenReturnsOnePlayer()
    {
        var application = CreateAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<GetPlayerListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(1);
    }
}
