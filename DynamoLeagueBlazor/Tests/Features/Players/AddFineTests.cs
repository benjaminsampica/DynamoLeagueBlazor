using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

internal class AddFineTests : IntegrationTestBase
{
    private const string _endpoint = "api/players/addfine";

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
        fine.Reason.Should().Be(stubRequest.FineReason);
        fine.Amount.Should().Be(FineUtilities.CalculateFineAmount(mockPlayer.ContractValue));
    }
}

internal class AddFineRequestValidatorTests : IntegrationTestBase
{
    private AddFineRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = _setupApplication.Services.GetRequiredService<AddFineRequestValidator>();
    }

    [TestCase(0, "Test", ExpectedResult = false, Description = "Invalid player id")]
    [TestCase(1, "", ExpectedResult = false, Description = "Invalid reason")]
    [TestCase(1, null, ExpectedResult = false, Description = "Invalid reason")]
    [TestCase(1, "Test", ExpectedResult = true, Description = "Valid")]
    public bool GivenDifferentRequests_ThenReturnsExpectedResult(int playerId, string reason)
    {
        var request = new AddFineRequest { PlayerId = playerId, FineReason = reason };

        var result = _validator.Validate(request);

        return result.IsValid;
    }
}

internal class FineDetailRequestValidatorTests : IntegrationTestBase
{
    private FineDetailRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = _setupApplication.Services.GetRequiredService<FineDetailRequestValidator>();
    }

    [TestCase(0, ExpectedResult = false, Description = "Invalid player id")]
    [TestCase(1, ExpectedResult = true, Description = "Valid")]
    public bool GivenDifferentRequests_ThenReturnsExpectedResult(int playerId)
    {
        var request = new FineDetailRequest { PlayerId = playerId };

        var result = _validator.Validate(request);

        return result.IsValid;
    }
}