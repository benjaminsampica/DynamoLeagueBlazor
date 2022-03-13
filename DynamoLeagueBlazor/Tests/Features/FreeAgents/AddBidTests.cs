using AutoBogus;
using DynamoLeagueBlazor.Client.Features.FreeAgents;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class AddBidTests : IntegrationTestBase
{
    private const string _endpoint = "api/freeagents/addbid";

    private static AddBidRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddBidRequest>()
            .RuleFor(f => f.Amount, (faker) => faker.Random.Int(min: 1));

        return faker.Generate();
    }

    [Fact]
    public async Task GET_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();
        var endpoint = AddBidRouteFactory.Create(CreateFakeValidRequest());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenReturnsTrue()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.Create(request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GET_GivenAnyAuthenticatedUser_WhenIsNotHighestBid_ThenReturnsFalse()
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
        var endpoint = AddBidRouteFactory.Create(request);
        mockPlayer.AddBid(int.MaxValue, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task POST_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(_endpoint, CreateFakeValidRequest());

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
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;

        var result = await client.PostAsJsonAsync(_endpoint, request);

        result.StatusCode.Should().Be(HttpStatusCode.OK);

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
    [InlineData(0, int.MaxValue, false)]
    [InlineData(1, 0, false)]
    public void GivenDifferentRequests_ThenReturnsExpectedResult(int playerId, int amount, bool expectedResult)
    {
        var request = new AddBidRequest { PlayerId = playerId, Amount = amount };

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task GivenDifferentBidAmounts_WhenAPlayerAlreadyHasBidOfOneDollar_ThenReturnsExpectedResult(int amount, bool expectedResult)
    {
        var stubTeam = CreateFakeTeam();
        await _setupApplication.AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        await _setupApplication.AddAsync(mockPlayer);
        mockPlayer.AddBid(1, stubTeam.Id);
        await _setupApplication.UpdateAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = amount };

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedResult);
    }
}