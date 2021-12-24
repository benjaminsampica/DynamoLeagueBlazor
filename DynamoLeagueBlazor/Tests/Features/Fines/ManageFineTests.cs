using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class ManageFineTests : IntegrationTestBase
{
    private const string _endpoint = "fines/manage";

    private static ManageFineRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<ManageFineRequest>()
            .RuleFor(f => f.FineId, 1);

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
    public async Task GivenAnyAuthenticatedUser_WhenFineIsApproved_ThenUpdatesIt()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        mockPlayer.AddFine(int.MaxValue, RandomString);
        await application.UpdateAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = true;

        var response = await client.PostAsJsonAsync(_endpoint, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
        fine!.Status.Should().BeTrue();
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenFineIsNotApproved_ThenDeletesIt()
    {
        var application = CreateUserAuthenticatedApplication();
        var client = application.CreateClient();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        mockPlayer.AddFine(int.MaxValue, RandomString);
        await application.UpdateAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = false;

        var response = await client.PostAsJsonAsync(_endpoint, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().BeNull();
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenAnInvalidFine_ThenReturnsBadRequestWithErrors()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var badRequest = new ManageFineRequest { FineId = -1 };
        var response = await client.PostAsJsonAsync(_endpoint, badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var details = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync());
        details.Should().NotBeNull();
        details!.Errors.Should().NotBeEmpty();
    }
}
