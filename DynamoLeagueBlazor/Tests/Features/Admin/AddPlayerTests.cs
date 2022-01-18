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
public class AddPlayerRequestValidatorTests
{
    private AddPlayerRequestValidator _validator = null!;
    [SetUp]
    public void SetUp()
    {
        _validator = new AddPlayerRequestValidator();
    }

    [TestCase(" ", ExpectedResult = false, Description = "Name is blank")]
    [TestCase(null, ExpectedResult = false, Description = "No name is given")]
    [TestCase("Test", ExpectedResult = true, Description = "Valid name")]
    public bool GivenDifferentPlayerNames_ThenReturnsExpectedResult(string name)
    {
        var request = new AddPlayerRequest() { Name = name };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.Name));

        return hasNoErrors;
    }
    [TestCase(" ", ExpectedResult = false, Description = "HeadShot is blank")]
    [TestCase(null, ExpectedResult = false, Description = "No HeadShot is given")]
    [TestCase("Test", ExpectedResult = true, Description = "Valid HeadShot")]
    public bool GivenDifferentPlayerHeadShots_ThenReturnsExpectedResult(string headShot)
    {
        var request = new AddPlayerRequest() { HeadShot = headShot };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.HeadShot));

        return hasNoErrors;
    }

    [TestCase(0, ExpectedResult = false, Description = "Contract value must be greater than 0")]
    [TestCase(1, ExpectedResult = true, Description = "Valid Contract Value")]
    public bool GivenDifferentPlayerContractValue_ThenReturnsExpectedResult(int contractValue)
    {
        var request = new AddPlayerRequest() { ContractValue = contractValue };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.ContractValue));

        return hasNoErrors;
    }
    [TestCase(0, ExpectedResult = false, Description = "Team Id must be greater than")]
    [TestCase(1, ExpectedResult = true, Description = "Valid Team Id")]
    public bool GivenDifferentPlayerTeamId_ThenReturnsExpectedResult(int teamId)
    {
        var request = new AddPlayerRequest() { TeamId = teamId };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.TeamId));

        return hasNoErrors;
    }

    [TestCase(" ", ExpectedResult = false, Description = "Position is blank")]
    [TestCase(null, ExpectedResult = false, Description = "No position is given")]
    [TestCase("TE", ExpectedResult = true, Description = "Valid Position")]
    public bool GivenDifferentPlayerPosition_ThenReturnsExpectedResult(string position)
    {
        var request = new AddPlayerRequest() { Position = position };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.Position));

        return hasNoErrors;
    }

}

