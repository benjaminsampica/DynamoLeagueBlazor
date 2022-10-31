﻿using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class ManageFineTests : IntegrationTestBase
{
    private static ManageFineRequest CreateFakeValidRequest()
    {
        var faker = new AutoFaker<ManageFineRequest>()
            .RuleFor(f => f.FineId, 1);

        return faker.Generate();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(ManageFineRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var stubRequest = CreateFakeValidRequest();
        var response = await client.PostAsJsonAsync(ManageFineRouteFactory.Uri, stubRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenFineIsApproved_ThenUpdatesIt()
    {
        var application = GetAdminAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await AddAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = true;
        mockRequest.FineId = mockFine.Id;

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(ManageFineRouteFactory.Uri, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await FirstOrDefaultAsync<Fine>();
        fine.Should().NotBeNull();
        fine!.Status.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenFineIsNotApproved_ThenDeletesIt()
    {
        var application = GetAdminAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = stubTeam.Id;
        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await AddAsync(mockPlayer);

        var mockRequest = CreateFakeValidRequest();
        mockRequest.Approved = false;
        mockRequest.FineId = mockFine.Id;

        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(ManageFineRouteFactory.Uri, mockRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fine = await FirstOrDefaultAsync<Fine>();
        fine.Should().BeNull();
    }
}

public class ManageFineRequestValidatorTests
{
    private readonly ManageFineRequestValidator _validator = null!;

    public ManageFineRequestValidatorTests()
    {
        _validator = new ManageFineRequestValidator();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void GivenDifferentRequests_ThenReturnsExpectedResult(int fineId, bool expectedResult)
    {
        var request = new ManageFineRequest { FineId = fineId };

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedResult);
    }
}