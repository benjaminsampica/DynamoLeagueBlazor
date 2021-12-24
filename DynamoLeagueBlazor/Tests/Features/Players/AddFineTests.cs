using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class AddFineTests : IntegrationTestBase
{
    private const string _endpoint = "players/addfine";

    private static AddFineRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<AddFineRequest>()
            .RuleFor(f => f.PlayerId, 1);

        return faker.Generate();
    }

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(_endpoint, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenAValidFine_ThenSavesIt()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);
        var stubRequest = CreateFakeValidRequest();
        stubRequest.PlayerId = mockPlayer.Id;

        var response = await client.PostAsJsonAsync(_endpoint, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
        fine!.PlayerId.Should().Be(stubRequest.PlayerId);
        fine.Status.Should().BeFalse();
        fine.FineReason.Should().Be(stubRequest.FineReason);
        fine.FineAmount.Should().Be(FineUtilities.CalculateFineAmount(mockPlayer.ContractValue));
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenAnInvalidFine_ThenReturnsBadRequestWithErrors()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var badRequest = new AddFineRequest { PlayerId = -1, FineReason = string.Empty };
        var response = await client.PostAsJsonAsync(_endpoint, badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var details = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync());
        details.Should().NotBeNull();
        details!.Errors.Should().NotBeEmpty();
    }
}
