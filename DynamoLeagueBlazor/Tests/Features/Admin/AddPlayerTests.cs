
using DynamoLeagueBlazor.Client.Features.Admin;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Teams;
using Moq;
using MudBlazor;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamNameListResult;
using Position = DynamoLeagueBlazor.Shared.Enums.Position;

namespace DynamoLeagueBlazor.Tests.Features.Admin;

internal class AddPlayerServerTests : IntegrationTestBase
{
    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(AddPlayerRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(AddPlayerRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_ThenAddsAPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();

        var team = CreateFakeTeam();
        await application.AddAsync(team);

        var request = new AddPlayerRequest
        {
            Name = RandomString,
            Position = Position.Defense.Name,
            HeadShot = RandomString,
            TeamId = team.Id,
            ContractValue = int.MaxValue
        };

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(AddPlayerRouteFactory.Uri, request);

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

internal class AddPlayerUITests : UITestBase
{
    private static readonly TeamNameItem _teamNameItem = new()
    {
        Id = int.MaxValue,
        Name = RandomString
    };

    private static readonly TeamNameListResult _teamNameListResult = new()
    {
        Teams = new List<TeamNameItem>
        {
            _teamNameItem
        }
    };

    [Test]
    public void WhenPageLoads_ThenPopulatesListOfTeams()
    {
        GetHttpHandler.When(HttpMethod.Get, AddPlayerRouteFactory.Uri)
            .RespondsWithJson(_teamNameListResult);

        var cut = RenderComponent<AddPlayer>();

        var teamSelectItem = cut.FindComponents<MudSelectItem<int>>()
            .FirstOrDefault(s => s.Instance.Value == _teamNameItem.Id);

        teamSelectItem.Should().NotBeNull();
    }

    [Test]
    public async Task GivenAValidForm_WhenSubmitIsClicked_ThenSavesTheForm()
    {
        GetHttpHandler.When(HttpMethod.Get, AddPlayerRouteFactory.Uri)
            .RespondsWithJson(_teamNameListResult);

        GetHttpHandler.When(HttpMethod.Post, AddPlayerRouteFactory.Uri)
            .Respond(message => Task.FromResult(message.CreateResponse(HttpStatusCode.OK)))
            .Verifiable();

        var cut = RenderComponent<AddPlayer>();

        // Fill the form and click submit.
        var playerName = cut.Find($"#{nameof(AddPlayerRequest.Name)}");
        playerName.Change(RandomString);

        var contractValue = cut.Find($"#{nameof(AddPlayerRequest.ContractValue)}");
        contractValue.Change(int.MaxValue);

        var position = cut.FindComponent<MudSelect<string>>();
        await position.InvokeAsync(async () => await position.Instance.ValueChanged.InvokeAsync(Position.Defense.Name));

        var team = cut.FindComponent<MudSelect<int>>();
        await team.InvokeAsync(async () => await team.Instance.ValueChanged.InvokeAsync(_teamNameItem.Id));

        var headShot = cut.Find($"#{nameof(AddPlayerRequest.HeadShot)}");
        headShot.Change(RandomString);

        var submitButton = cut.Find("button");
        submitButton.Click();

        MockSnackbar.Verify(s => s.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>()));
    }
}

internal class AddPlayerRequestValidatorTests
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
    [TestCase(null, ExpectedResult = false, Description = "No headShot is given")]
    [TestCase("Test", ExpectedResult = true, Description = "Valid headShot")]
    public bool GivenDifferentPlayerHeadShots_ThenReturnsExpectedResult(string headShot)
    {
        var request = new AddPlayerRequest() { HeadShot = headShot };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.HeadShot));

        return hasNoErrors;
    }

    [TestCase(0, ExpectedResult = false, Description = "Contract value must be greater than 0")]
    [TestCase(1, ExpectedResult = true, Description = "Valid contract value")]
    public bool GivenDifferentPlayerContractValue_ThenReturnsExpectedResult(int contractValue)
    {
        var request = new AddPlayerRequest() { ContractValue = contractValue };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.ContractValue));

        return hasNoErrors;
    }

    [TestCase(0, ExpectedResult = false, Description = "Team Id must be greater than")]
    [TestCase(1, ExpectedResult = true, Description = "Valid team Id")]
    public bool GivenDifferentPlayerTeamId_ThenReturnsExpectedResult(int teamId)
    {
        var request = new AddPlayerRequest() { TeamId = teamId };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.TeamId));

        return hasNoErrors;
    }

    [TestCase(" ", ExpectedResult = false, Description = "Position is blank")]
    [TestCase(null, ExpectedResult = false, Description = "No position is given")]
    [TestCase("POE", ExpectedResult = false, Description = "Invalid position")]
    [TestCase("TE", ExpectedResult = true, Description = "Valid position")]
    public bool GivenDifferentPlayerPosition_ThenReturnsExpectedResult(string position)
    {
        var request = new AddPlayerRequest() { Position = position };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.Position));

        return hasNoErrors;
    }

}

