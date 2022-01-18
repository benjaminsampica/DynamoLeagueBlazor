using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Enums;
using DynamoLeagueBlazor.Shared.Features.Admin;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Admin;

public class AddPlayerTests : IntegrationTestBase
{
    private const string _endpoint = "api/admin/addplayer";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_ThenAddsAPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();

        var team = CreateFakeTeam();
        await application.AddAsync(team);

        var request = new AddPlayerRequest() { Name = RandomString, Position = Position.Defense.Name, HeadShot = RandomString, TeamId = team.Id, ContractValue = int.MaxValue };

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(_endpoint, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var player = await application.FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.Name.Should().Be(request.Name);
        player.Position.Should().Be(request.Position);
        player.HeadShotUrl.Should().Be(request.HeadShot);
        player.TeamId.Should().Be(team.Id);
        player.ContractValue.Should().Be(request.ContractValue);
    }
}
public class AddPlayerValidatorTests
{ 


}

