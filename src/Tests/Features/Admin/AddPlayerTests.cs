
using DynamoLeagueBlazor.Client.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using Microsoft.Extensions.DependencyInjection;
using static DynamoLeagueBlazor.Shared.Features.Admin.TeamNameListResult;
using Position = DynamoLeagueBlazor.Shared.Enums.Position;

namespace DynamoLeagueBlazor.Tests.Features.Admin;

public class AddPlayerServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(AddPlayerRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(AddPlayerRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_ThenAddsAPlayer()
    {
        var application = GetAdminAuthenticatedApplication();

        var team = CreateFakeTeam();
        await AddAsync(team);

        var request = new AddPlayerRequest
        {
            Name = RandomString,
            Position = Position.Defense.Name,
            TeamId = team.Id,
            ContractValue = int.MaxValue
        };

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(AddPlayerRouteFactory.Uri, request);

        response.Should().BeSuccessful();

        var player = await FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.Name.Should().Be(request.Name);
        player.Position.Should().Be(request.Position);
        player.HeadShotUrl.Should().NotBeEmpty();
        player.TeamId.Should().Be(team.Id);
        player.ContractValue.Should().Be(request.ContractValue);
    }
}

public class AddPlayerUITests : UITestBase
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

    public AddPlayerUITests()
    {
        TestContext!.Services.AddSingleton(Mock.Of<IPlayerHeadshotService>());
    }

    [Fact]
    public void WhenPageLoads_ThenPopulatesListOfTeams()
    {
        GetHttpHandler.When(HttpMethod.Get, AddPlayerRouteFactory.GetTeamListUri)
            .RespondsWithJson(_teamNameListResult);

        var cut = RenderComponent<AddPlayer>();

        var teamSelectItem = cut.FindComponents<MudSelectItem<int>>()
            .FirstOrDefault(s => s.Instance.Value == _teamNameItem.Id);

        teamSelectItem.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAValidForm_WhenSubmitIsClicked_ThenSavesTheForm()
    {
        GetHttpHandler.When(HttpMethod.Get, AddPlayerRouteFactory.GetTeamListUri)
            .RespondsWithJson(_teamNameListResult);

        GetHttpHandler.When(HttpMethod.Post, AddPlayerRouteFactory.Uri)
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
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

        var submitButton = cut.Find("button");
        submitButton.Click();

        MockSnackbar.Verify(s => s.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()));
        GetHttpHandler.Verify();
    }
}

public class AddPlayerRequestValidatorTests
{
    private readonly AddPlayerRequestValidator _validator = null!;

    public AddPlayerRequestValidatorTests()
    {
        _validator = new AddPlayerRequestValidator();
    }

    [Theory]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    [InlineData("Test", true)]
    public void GivenDifferentPlayerNames_ThenReturnsExpectedResult(string name, bool expectedResult)
    {
        var request = new AddPlayerRequest() { Name = name };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.Name));

        hasNoErrors.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void GivenDifferentPlayerContractValue_ThenReturnsExpectedResult(int contractValue, bool expectedResult)
    {
        var request = new AddPlayerRequest() { ContractValue = contractValue };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.ContractValue));

        hasNoErrors.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void GivenDifferentPlayerTeamId_ThenReturnsExpectedResult(int teamId, bool expectedResult)
    {
        var request = new AddPlayerRequest() { TeamId = teamId };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.TeamId));

        hasNoErrors.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    [InlineData("POE", false)]
    [InlineData("TE", true)]
    public void GivenDifferentPlayerPosition_ThenReturnsExpectedResult(string position, bool expectedResult)
    {
        var request = new AddPlayerRequest() { Position = position };

        var result = _validator.Validate(request);
        var hasNoErrors = result.Errors.All(e => e.PropertyName != nameof(AddPlayerRequest.Position));

        hasNoErrors.Should().Be(expectedResult);
    }

}

