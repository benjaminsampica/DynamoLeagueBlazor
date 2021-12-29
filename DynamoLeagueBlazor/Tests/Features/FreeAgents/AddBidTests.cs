using AutoBogus;
using DynamoLeagueBlazor.Client.Features.FreeAgents;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

internal class AddBidTests : IntegrationTestBase
{
    private static AddBidRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddBidRequest>()
            .RuleFor(f => f.Amount, (faker) => faker.Random.Int(min: 1));

        return faker.Generate();
    }

    [Test]
    public async Task GET_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();
        var endpoint = AddBidRouteFactory.CreateRequestUri(CreateFakeValidRequest());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GET_GivenAnyAuthenticatedUser_WhenIsHighestBid_ThenReturnsTrue()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();

        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var request = CreateFakeValidRequest();
        request.PlayerId = mockPlayer.Id;
        var endpoint = AddBidRouteFactory.CreateRequestUri(request);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeTrue();
    }

    [Test]
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
        var endpoint = AddBidRouteFactory.CreateRequestUri(request);
        mockPlayer.AddBid(int.MaxValue, mockTeam.Id);
        await application.UpdateAsync(mockPlayer);

        var result = await client.GetFromJsonAsync<bool>(endpoint);
        result.Should().BeFalse();
    }

    [Test]
    public async Task POST_GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var client = application.CreateClient();

        var endpoint = AddBidRouteFactory.CreateRequestUri(CreateFakeValidRequest());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
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
        var endpoint = AddBidRouteFactory.CreateRequestUri(request);

        var result = await client.PostAsJsonAsync(endpoint, request);

        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var bid = await application.FirstOrDefaultAsync<Bid>();
        bid.Should().NotBeNull();
        bid!.Amount.Should().Be(request.Amount);
        bid.PlayerId.Should().Be(request.PlayerId);
        bid.TeamId.Should().Be(UserAuthenticationHandler.TeamId);
        bid.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}

internal class AddBidRequestValidatorTests : IntegrationTestBase
{
    private AddBidRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = _setupApplication.Services.GetRequiredService<AddBidRequestValidator>();
    }

    [TestCase(0, int.MaxValue, ExpectedResult = false, Description = "Invalid player id")]
    [TestCase(1, 1, ExpectedResult = false, Description = "Invalid amount")]
    public bool GivenDifferentRequests_ThenReturnsExpectedResult(int playerId, int amount)
    {
        var request = new AddBidRequest { PlayerId = playerId, Amount = amount };

        var result = _validator.Validate(request);

        return result.IsValid;
    }

    [TestCase(1, ExpectedResult = false, Description = "Bid is exactly the same amount")]
    [TestCase(2, ExpectedResult = true, Description = "Bid is one dollar higher")]
    public async Task<bool> GivenDifferentBidAmounts_WhenAPlayerHasOutstandingBidOfOneDollar_ThenReturnsExpectedResult(int amount)
    {
        var stubTeam = CreateFakeTeam();
        await _setupApplication.AddAsync(stubTeam);
        var mockPlayer = CreateFakePlayer();
        await _setupApplication.AddAsync(mockPlayer);
        mockPlayer.AddBid(1, stubTeam.Id);
        await _setupApplication.UpdateAsync(mockPlayer);

        var request = new AddBidRequest { PlayerId = mockPlayer.Id, Amount = amount };

        var result = _validator.Validate(request);

        return result.IsValid;
    }
}