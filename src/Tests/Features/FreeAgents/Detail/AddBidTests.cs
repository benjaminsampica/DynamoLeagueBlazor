using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents.Detail;

public class AddBidTests : IntegrationTestBase
{
    private static AddBidRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddBidRequest>()
            .RuleFor(f => f.Amount, (faker) => faker.Random.Int(min: Bid.MinimumAmount));

        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();
        var client = application.CreateClient();
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, CreateFakeValidRequest());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenReturnsTrue()
    {
        var application = GetUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        await AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenIsNotHighestBid_ThenReturnsFalse()
    {
        var application = GetUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);
        var mockPlayer = CreateFakePlayer();
        await AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        request.Amount = int.MinValue;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, request);
        mockPlayer.AddBid(int.MaxValue, mockTeam.Id);
        await UpdateAsync(mockPlayer);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenHasNotEnded_ThenReturnsTrue()
    {
        var application = GetUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddSeconds(1);
        await AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetHasNotEndedUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenHasEnded_ThenReturnsFalse()
    {
        var application = GetUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddSeconds(-1);
        await AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(AddBidRouteFactory.GetHasNotEndedUri, request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task POST_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(AddBidRouteFactory.Uri, CreateFakeValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenSavesTheBid()
    {
        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(1);
        await AddAsync(mockPlayer);

        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        request.Amount = Bid.MinimumAmount;

        var application = GetUserAuthenticatedApplication(mockTeam.Id);
        var client = application.CreateClient();

        var result = await client.PostAsJsonAsync(AddBidRouteFactory.Uri, request);

        result.Should().BeSuccessful();

        var bid = await FirstOrDefaultAsync<Bid>();
        bid.Should().NotBeNull();
        bid!.Amount.Should().Be(request.Amount);
        bid.PlayerId.Should().Be(request.PlayerId);
        bid.TeamId.Should().Be(mockTeam.Id);
        bid.CreatedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task POST_GivenAnyAuthenticatedUser_WhenIsHighestPublicBid_ButThereIsAnExistingOverBidWithAHigherAmount_ThenSavesThreeBidsWithTheOverBidAsTheHighest()
    {
        var originalTeam = CreateFakeTeam();
        await AddAsync(originalTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddDays(1);
        await AddAsync(mockPlayer);

        const int privateBidAmount = 2;
        mockPlayer.AddBid(privateBidAmount, originalTeam.Id);
        await UpdateAsync(mockPlayer);

        var biddingTeam = CreateFakeTeam();
        await AddAsync(biddingTeam);
        var biddingTeamRequest = CreateFakeValidRequest();
        biddingTeamRequest.PlayerId = mockPlayer.Id;
        biddingTeamRequest.Amount = privateBidAmount;

        var application = GetUserAuthenticatedApplication(biddingTeam.Id);
        var client = application.CreateClient();

        var result = await client.PostAsJsonAsync(AddBidRouteFactory.Uri, biddingTeamRequest);

        result.Should().BeSuccessful();

        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Bids.Should().HaveCount(3);

        dbContext.Bids.FindHighestBid()!.Amount.Should().Be(2);
    }
}

public class AddBidRequestValidatorTests : IntegrationTestBase
{
    private readonly AddBidRequestValidator _validator = null!;

    public AddBidRequestValidatorTests()
    {
        _validator = GetRequiredService<AddBidRequestValidator>();
    }

    [Theory]
    [InlineData(-1), InlineData(0)]
    public async Task GivenInvalidPlayerIds_ThenAreNotValid(int playerId) =>
        (await new AddBidRequestValidator(Mock.Of<IBidValidator>()).TestValidateAsync(new AddBidRequest { PlayerId = playerId }))
        .ShouldHaveValidationErrorFor(p => p.PlayerId);

    [Theory]
    [InlineData(-1), InlineData(0)]
    public async Task GivenInvalidAmounts_ThenAreNotValid(int amount) =>
        (await new AddBidRequestValidator(Mock.Of<IBidValidator>()).TestValidateAsync(new AddBidRequest { Amount = amount, PlayerId = int.MaxValue }))
        .ShouldHaveValidationErrorFor(p => p.Amount);

    [Fact]
    public async Task GivenABidOfOne_WhenAPlayerAlreadyHasBidOfOneDollar_ThenIsNotValid()
    {
        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        await AddAsync(mockPlayer);
        mockPlayer.AddBid(1, stubTeam.Id);
        await UpdateAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = 1 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(p => p.Amount);
    }

    [Fact]
    public async Task GivenABidOfTwo_WhenAPlayerAlreadyHasBidOfOneDollar_ThenIsValid()
    {
        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        await AddAsync(mockPlayer);
        mockPlayer.AddBid(1, stubTeam.Id);
        await UpdateAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = 2 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(p => p.Amount);
    }

    [Fact]
    public async Task GivenAPlayerEndOfFreeAgencyOfNow_WhenItIsNow_ThenIsNotValid()
    {
        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow;
        await AddAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = int.MaxValue };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(p => p);
    }

    [Fact]
    public async Task GivenAPlayerEndOfFreeAgencyOfNowPlusOneSecond_WhenItIsNow_ThenIsValid()
    {
        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        mockPlayer.EndOfFreeAgency = DateTimeOffset.UtcNow.AddSeconds(1);
        await AddAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = int.MaxValue };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(p => p);
    }
}