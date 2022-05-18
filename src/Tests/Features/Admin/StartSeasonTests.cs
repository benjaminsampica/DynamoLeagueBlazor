using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Admin;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class StartSeasonTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsAFreeAgent_ThenReturnsTrue()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.SetToFreeAgent(DateTime.MaxValue);
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(StartSeasonRouteFactory.Uri);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenNotPlayerIsAFreeAgent_ThenReturnsFalse()
    {
        var application = CreateAdminAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(StartSeasonRouteFactory.Uri);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAFineExistsBeforeJanuary1stOfTheCurrentYear_ThenTheFineIsRemoved()
    {
        var application = CreateAdminAuthenticatedApplication();
        var stubPlayer = CreateFakePlayer();
        await application.AddAsync(stubPlayer);
        var mockFine = CreateFakeFine(stubPlayer.Id);
        await application.AddAsync(mockFine);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().BeNull();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsEligibleForFreeAgency_ThenSetsPlayerToFreeAgent()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.YearContractExpires = null;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await application.FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.EndOfFreeAgency.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsNotEligibleForFreeAgency_ThenSkipsThatPlayer()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.YearContractExpires = int.MaxValue;
        mockPlayer.EndOfFreeAgency = null;
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.PostAsync(StartSeasonRouteFactory.Uri, null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var player = await application.FirstOrDefaultAsync<Player>();
        player.Should().NotBeNull();
        player!.EndOfFreeAgency.Should().BeNull();
    }
}
