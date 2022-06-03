
using DynamoLeagueBlazor.Client.Features.Admin;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Admin.TeamNameListResult;
using Position = DynamoLeagueBlazor.Shared.Enums.Position;

namespace DynamoLeagueBlazor.Tests.Features.Admin;

public class AddPlayerServerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(AddPlayerRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(AddPlayerRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_ThenAddsAPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();

        var team = CreateFakeTeam();
        await application.AddAsync(team);

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

        var player = await application.FirstOrDefaultAsync<Player>();
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

    [Fact]
    public void WhenPageLoads_ThenPopulatesListOfTeams()
    {
        TestContext!.Services.AddSingleton(Mock.Of<IPlayerHeadshotService>());

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
        var mockPlayerHeadshotService = new Mock<IPlayerHeadshotService>();
        mockPlayerHeadshotService.Setup(phs => phs.FindPlayerHeadshotUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RandomString);
        TestContext!.Services.AddSingleton(mockPlayerHeadshotService.Object);

        GetHttpHandler.When(HttpMethod.Get, AddPlayerRouteFactory.GetTeamListUri)
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

        var submitButton = cut.Find("button");
        submitButton.Click();

        MockSnackbar.Verify(s => s.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>()));
        GetHttpHandler.Verify();
    }
}

public class AddPlayerRequestValidatorTests
{
    private readonly AddPlayerRequestValidator _validator = null!;

    public AddPlayerRequestValidatorTests()
    {
        _validator = new AddPlayerRequestValidator(Mock.Of<IPlayerHeadshotService>());
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

