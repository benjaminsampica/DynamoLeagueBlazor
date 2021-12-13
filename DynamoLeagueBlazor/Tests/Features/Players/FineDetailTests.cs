using AutoBogus;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
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
        var endpoint = $"{_endpoint}/{stubRequest.PlayerId}";

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenPlayerIdIsFound_ThenReturnsExpectedResult()
    {
        var application = CreateAuthenticatedApplication();

        var client = application.CreateClient();

        var player = CreateFakePlayer();
        await application.AddAsync(player);
        var mockRequest = CreateFakeRequest();
        mockRequest.PlayerId = 1;
        var endpoint = $"{_endpoint}?PlayerId={player.Id}";

        var response = await client.GetFromJsonAsync<FineDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.FineAmount.Should().Be(FineUtilities.CalculateFineAmount(player.ContractValue).ToString("C0"));
        response.ContractValue.Should().Be(player.ContractValue.ToString("C0"));
    }
}
