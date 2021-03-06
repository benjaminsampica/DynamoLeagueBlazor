using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.Extensions.DependencyInjection;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class AddBidTests : IntegrationTestBase
{
    private static AddBidRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddBidRequest>()
            .RuleFor(f => f.Amount, (faker) => faker.Random.Int(min: 1));

        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, CreateFakeValidRequest());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenReturnsTrue()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenIsNotHighestBid_ThenReturnsFalse()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        request.Amount = int.MinValue;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, request);
        mockPlayer.AddBid(int.MaxValue, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenHasNotEnded_ThenReturnsTrue()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Now.AddSeconds(1);
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetHasNotEndedUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenHasEnded_ThenReturnsFalse()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Now.AddSeconds(-1);
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetHasNotEndedUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task POST_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(AddBidRouteFactory.Uri, CreateFakeValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenSavesTheBid()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockTeam = CreateFakeTeam();
        await application.AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Now.AddDays(1);
        await application.AddAsync(mockPlayer);

        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;

        var result = await client.PostAsJsonAsync(AddBidRouteFactory.Uri, request);

        result.Should().BeSuccessful();

        var bid = await application.FirstOrDefaultAsync<Bid>();
        bid.Should().NotBeNull();
        bid!.Amount.Should().Be(request.Amount);
        bid.PlayerId.Should().Be(request.PlayerId);
        bid.TeamId.Should().Be(UserAuthenticationHandler.TeamId);
        bid.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}

public class AddBidRequestValidatorTests : IntegrationTestBase
{
    private readonly AddBidRequestValidator _validator = null!;

    public AddBidRequestValidatorTests()
    {
        _validator = _setupApplication.Services.GetRequiredService<AddBidRequestValidator>();
    }

    [Theory]
    [InlineData(-1), InlineData(0)]
    public void GivenInvalidPlayerIds_ThenAreNotValid(int playerId) =>
        new AddBidRequestValidator(Mock.Of<IBidValidator>()).TestValidate(new AddBidRequest { PlayerId = playerId }).ShouldHaveValidationErrorFor(p => p.PlayerId);

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task GivenDifferentBidAmounts_WhenAPlayerAlreadyHasBidOfOneDollar_ThenReturnsExpectedResult(int amount, bool expectedResult)
    {
        var stubTeam = CreateFakeTeam();
        await _setupApplication.AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.AddBid(1, stubTeam.Id);
        await _setupApplication.UpdateAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = amount };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GivenAPlayerEndOfFreeAgencyOfNow_WhenItIsNow_ThenIsNotValid()
    {
        var stubTeam = CreateFakeTeam();
        await _setupApplication.AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Now;
        await _setupApplication.AddAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = int.MaxValue };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(p => p);
    }

    [Fact]
    public async Task GivenAPlayerEndOfFreeAgencyOfNowPlusOneSecond_WhenItIsNow_ThenIsValid()
    {
        var stubTeam = CreateFakeTeam();
        await _setupApplication.AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTime.Now.AddSeconds(1);
        await _setupApplication.AddAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = int.MaxValue };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(p => p);
    }
}