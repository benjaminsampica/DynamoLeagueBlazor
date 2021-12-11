using DynamoLeagueBlazor.Shared.Features.Teams;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "teams";

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
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneTeam_ThenReturnsOneTeam()
    {
        var application = CreateAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TeamListResult>(_endpoint);

        result.Should().NotBeNull();
        result!.Teams.Should().HaveCount(1);
    }
}
