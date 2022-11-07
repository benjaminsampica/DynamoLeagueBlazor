using DynamoLeagueBlazor.Client.Features.Teams;
using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class AddFineServerTests : IntegrationTestBase
{
    private static AddTeamFineRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddTeamFineRequest>()
            .RuleFor(f => f.TeamId, 1)
            .RuleFor(f => f.Amount, int.MaxValue);

        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(AddTeamFineRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(AddTeamFineRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenAValidFine_ThenSavesIt()
    {
        var application = GetAdminAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.TeamId = stubTeam.Id;

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(AddTeamFineRouteFactory.Uri, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fine = await FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
        fine!.TeamId.Should().Be(mockRequest.TeamId);
        fine.Status.Should().BeFalse();
        fine.Reason.Should().Be(mockRequest.FineReason);
        fine.Amount.Should().Be(mockRequest.Amount);
    }
}

public class AddFineUITests : UITestBase
{
    [Fact]
    public async Task GivenAnInvalidForm_ThenDoesNotSubmit()
    {
        GetHttpHandler.When(HttpMethod.Post, AddTeamFineRouteFactory.Uri)
            .Verifiable();

        var cut = await RenderMudDialogAsync<AddFine>();

        var submitButton = cut.Find("button");
        submitButton.Click();

        GetHttpHandler.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task GivenAValidForm_WhenSubmitIsClicked_ThenSavesTheForm()
    {
        GetHttpHandler.When(HttpMethod.Post, AddTeamFineRouteFactory.Uri)
            .Respond(message => Task.FromResult(message.CreateResponse(HttpStatusCode.OK)))
            .Verifiable();

        var cut = await RenderMudDialogAsync<AddFine>(new DialogParameters
        {
            { nameof(AddFine.TeamId), int.MaxValue }
        });

        // Fill the form and click submit.
        var amount = cut.Find($"#{nameof(AddTeamFineRequest.Amount)}");
        amount.Change(RandomPositiveNumber);

        var fineReason = cut.Find($"#{nameof(AddTeamFineRequest.FineReason)}");
        fineReason.Change(RandomString);

        var submitButton = cut.Find("button");
        submitButton.Click();

        MockSnackbar.Verify(s => s.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>()));
        GetHttpHandler.Verify();
    }
}

#pragma warning disable xUnit1012 // Null should not be used for value type parameters
public class AddFineRequestValidatorTests : IntegrationTestBase
{
    private readonly AddTeamFineRequestValidator _validator = null!;

    public AddFineRequestValidatorTests()
    {
        _validator = GetRequiredService<AddTeamFineRequestValidator>();

    }

    [Theory]
    [InlineData(null), InlineData(-1), InlineData(0)]
    public void GivenInvalidTeamIds_ThenAreNotValid(int teamId)
        => _validator.TestValidate(new AddTeamFineRequest { TeamId = teamId }).ShouldHaveValidationErrorFor(v => v.TeamId);

    [Fact]
    public void GivenAValidTeamId_ThenIsValid()
        => _validator.TestValidate(new AddTeamFineRequest { TeamId = 1 }).ShouldNotHaveValidationErrorFor(v => v.TeamId);

    [Theory]
    [InlineData(null), InlineData("")]
    public void GivenInvalidFineReasons_ThenIsNotValid(string fineReason)
        => _validator.TestValidate(new AddTeamFineRequest { FineReason = fineReason }).ShouldHaveValidationErrorFor(v => v.FineReason);

    [Fact]
    public void GivenAValidFineReason_ThenIsValid()
        => _validator.TestValidate(new AddTeamFineRequest { FineReason = RandomString }).ShouldNotHaveValidationErrorFor(v => v.FineReason);

    [Theory]

    [InlineData(null), InlineData(-1), InlineData(0)]
    public void GivenInvalidAmounts_ThenAreNotValid(int amount)
        => _validator.TestValidate(new AddTeamFineRequest { Amount = amount }).ShouldHaveValidationErrorFor(v => v.Amount);

    [Fact]
    public void GivenAValidAmount_ThenIsValid()
        => _validator.TestValidate(new AddTeamFineRequest { Amount = 1 }).ShouldNotHaveValidationErrorFor(v => v.Amount);
}
#pragma warning restore xUnit1012 // Null should not be used for value type parameters