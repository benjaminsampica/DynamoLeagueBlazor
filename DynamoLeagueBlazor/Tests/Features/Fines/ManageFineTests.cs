using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class ManageFineTests : IntegrationTestBase
{
    private const string _endpoint = "api/fines/manage";

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
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(_endpoint, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_WhenFineIsApproved_ThenUpdatesIt()
    {
        var application = CreateAdminAuthenticatedApplication();
        var client = application.CreateClient();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await application.UpdateAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = true;
        mockRequest.FineId = mockFine.Id;

        var response = await client.PostAsJsonAsync(_endpoint, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
        fine!.Status.Should().BeTrue();
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_WhenFineIsNotApproved_ThenDeletesIt()
    {
        var application = CreateAdminAuthenticatedApplication();
        var client = application.CreateClient();
        var mockPlayer = CreateFakePlayer();
        await application.AddAsync(mockPlayer);

        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await application.UpdateAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = false;
        mockRequest.FineId = mockFine.Id;

        var response = await client.PostAsJsonAsync(_endpoint, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await application.FirstOrDefaultAsync<Fine>();
        fine.Should().BeNull();
    }
}

internal class ManageFineRequestValidatorTests
{
    private ManageFineRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ManageFineRequestValidator();
    }

    [TestCase(0, ExpectedResult = false)]
    [TestCase(1, ExpectedResult = true)]
    public bool GivenDifferentRequests_ThenReturnsExpectedResult(int fineId)
    {
        var request = new ManageFineRequest { FineId = fineId };

        var result = _validator.Validate(request);

        return result.IsValid;
    }
}