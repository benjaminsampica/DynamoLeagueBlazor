using AutoBogus;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Players;

internal class FineDetailTests : IntegrationTestBase
{
    private const string _endpoint = "players/finedetail";

    private static FineDetailRequest CreateFakeRequest()
    {
        var faker = new AutoFaker<FineDetailRequest>()
            .RuleFor(p => p.PlayerId, 1);

        return faker.Generate();
    }

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeRequest();
        var endpoint = QueryHelpers.AddQueryString(_endpoint, nameof(FineDetailRequest.PlayerId), stubRequest.PlayerId.ToString());

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerIsFound_ThenReturnsExpectedResult()
    {
        var application = CreateAuthenticatedApplication();

        var client = application.CreateClient();

        var player = CreateFakePlayer();
        await application.AddAsync(player);
        var mockRequest = CreateFakeRequest();
        mockRequest.PlayerId = player.Id;

        var endpoint = QueryHelpers.AddQueryString(_endpoint, nameof(FineDetailRequest.PlayerId), mockRequest.PlayerId.ToString());

        var response = await client.GetFromJsonAsync<FineDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.FineAmount.Should().Be(FineUtilities.CalculateFineAmount(player.ContractValue).ToString("C0"));
        response.ContractValue.Should().Be(player.ContractValue.ToString("C0"));
    }
}
