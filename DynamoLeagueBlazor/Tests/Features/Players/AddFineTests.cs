using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
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
    public async Task GivenAnyAuthenticatedUser_GivenAValidFine_ThenSavesIt()
    {
        var application = CreateAuthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(_endpoint, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fine = await application.FindAsync<Fine>(1);
        fine.Should().NotBeNull();
        fine!.PlayerId.Should().Be(stubRequest.PlayerId);
        fine.Status.Should().BeFalse();
        fine.FineReason.Should().Be(stubRequest.FineReason);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_GivenAnInvalidFine_ThenReturnsBadRequestWithErrors()
    {
        var application = CreateAuthenticatedApplication();

        var client = application.CreateClient();

        var badRequest = new AddFineRequest { PlayerId = -1, FineReason = string.Empty };
        var response = await client.PostAsJsonAsync(_endpoint, badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var details = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync());
        details.Should().NotBeNull();
        details!.Errors.Should().NotBeEmpty();
    }
}
