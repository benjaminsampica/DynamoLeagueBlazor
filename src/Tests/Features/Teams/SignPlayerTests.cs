using DynamoLeagueBlazor.Client.Features.Teams;
using DynamoLeagueBlazor.Shared.Enums;
using DynamoLeagueBlazor.Shared.Features.Teams;
using Position = DynamoLeagueBlazor.Shared.Enums.Position;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class SignPlayerServerTests : IntegrationTestBase
{
    private static SignPlayerRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<SignPlayerRequest>();
        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(SignPlayerRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenSignsPlayer()
    {
        var application = GetUserAuthenticatedApplication();
        var player = CreateFakePlayer();
        player.Position = Position.QuarterBack.Name;
        player.YearContractExpires = DateTime.Now.Year;
        await AddAsync(player);
        var request = CreateFakeValidRequest();
        request.YearContractExpires = (int)player.YearContractExpires;
        request.PlayerId = player.Id;
        var client = application.CreateClient();

        await client.PostAsJsonAsync(SignPlayerRouteFactory.Uri, request);

        var result = await FirstOrDefaultAsync<Player>();

        result.Should().NotBeNull();
        result!.YearContractExpires.Should().Be(request.YearContractExpires);
        result.EndOfFreeAgency.Should().BeNull();
        var position = Position.FromName(player.Position);
        var expectedContractValue = position.GetContractValue(request.YearContractExpires, player.ContractValue);
        result.ContractValue.Should().Be(expectedContractValue);
    }
}

public class SignPlayerUITests : UITestBase
{
    [Fact]
    public void WhenLoading_ThenShowsSkeleton()
    {
        // Delay the response.
        GetHttpHandler.When(HttpMethod.Get)
            .TimesOutAfter(5000);
        var component = RenderComponent<SignPlayer>();

        component.HasComponent<MudDialog>().Should().BeTrue();
    }

    [Fact]
    public void WhenLoads_ThenCallsForContractOptions_AndShowsTheForm()
    {
        var contractOptions = new SignPlayerDetailResult { ContractOptions = new List<ContractOption>() { new ContractOption(int.MaxValue, int.MaxValue) } };
        GetHttpHandler.When(HttpMethod.Get)
            .RespondsWithJson(contractOptions)
            .Verifiable();

        var component = RenderComponent<SignPlayer>();

        GetHttpHandler.Verify();
        component.Markup.Contains("form");
    }
}

public class SignPlayerRequestValidatorTests
{
    private readonly SignPlayerRequestValidator _validator = null!;

    public SignPlayerRequestValidatorTests()
    {
        _validator = new SignPlayerRequestValidator();
    }

    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    public void GivenDifferentRequests_ThenReturnsExpectedResult(int playerId, int yearContractExpires, bool expectedResult)
    {
        var request = new SignPlayerRequest { PlayerId = playerId, YearContractExpires = yearContractExpires };

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedResult);
    }
}
