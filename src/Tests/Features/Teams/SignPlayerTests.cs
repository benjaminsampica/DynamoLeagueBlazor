using DynamoLeagueBlazor.Client.Features.Teams;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Enums;
using DynamoLeagueBlazor.Shared.Features.Teams;
using MudBlazor;
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
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(SignPlayerRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_ThenSignsPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();
        var player = CreateFakePlayer();
        player.Position = Position.QuarterBack.Name;
        player.YearContractExpires = DateTime.Now.Year;
        await application.AddAsync(player);
        var request = CreateFakeValidRequest();
        request.YearContractExpires = (int)player.YearContractExpires;
        request.PlayerId = player.Id;
        var client = application.CreateClient();

        await client.PostAsJsonAsync(SignPlayerRouteFactory.Uri, request);

        var result = await application.FirstOrDefaultAsync<Player>();

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
            .Respond(async () =>
            {
                await Task.Delay(2000);

                return new HttpResponseMessage(HttpStatusCode.OK);
            });
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
