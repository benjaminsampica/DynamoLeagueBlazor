using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Teams;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

internal class ListTests : IntegrationTestBase
{
    private const string _endpoint = "/teams/list";
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
        var team = new Team("Test", "Test", "Test");
        await application.AddAsync(team);

        var client = application.CreateClient();

        var response = await client.GetFromJsonAsync<GetTeamListResult>(_endpoint);

        response.Should().NotBeNull();
        response!.Teams.Should().HaveCount(1);
    }
}
