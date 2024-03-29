﻿using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class ListTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(FineListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOnePlayerFine_ThenReturnsOnePlayerFine()
    {
        var application = GetUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var mockPlayer = CreateFakePlayer();
        mockPlayer.TeamId = mockTeam.Id;
        await AddAsync(mockPlayer);

        var mockFine = mockPlayer.AddFine(int.MaxValue, RandomString);
        await UpdateAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FineListResult>(FineListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Fines.Should().HaveCount(1);

        var fine = result!.Fines.First();
        fine.Id.Should().Be(mockFine.Id);
        fine.PlayerHeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
        fine.PlayerName.Should().Be(mockPlayer.Name);
        fine.Status.Should().BeOneOf("Pending", "Approved");
        fine.Amount.Should().Be(mockFine.Amount);
        fine.Reason.Should().Be(mockFine.Reason);
        fine.TeamName.Should().Be(mockTeam.Name);
        fine.TeamLogoUrl.Should().Be(mockTeam.LogoUrl);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneTeamFine_ThenReturnsOneTeamFine()
    {
        var application = GetUserAuthenticatedApplication();

        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        mockTeam.AddFine(int.MaxValue, RandomString);
        await UpdateAsync(mockTeam);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<FineListResult>(FineListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Fines.Should().HaveCount(1);

        var fine = result!.Fines.First();
        fine.PlayerHeadShotUrl.Should().Be(null);
        fine.PlayerName.Should().Be(null);
    }
}
