﻿using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

public class ListTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();
        var client = application.CreateClient();

        var response = await client.GetAsync(TeamListRouteFactory.Uri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAnyAuthenticatedUser_WhenThereIsOneTeam_ThenReturnsOneTeamWithPlayerCounts()
    {
        var application = GetUserAuthenticatedApplication();
        var mockTeam = CreateFakeTeam();
        await AddAsync(mockTeam);

        var rosteredPlayer = CreateFakePlayer();
        rosteredPlayer.TeamId = mockTeam.Id;
        rosteredPlayer.State = PlayerState.Rostered;
        await AddAsync(rosteredPlayer);

        var unrosteredPlayer = CreateFakePlayer().SetToUnrostered();
        unrosteredPlayer.TeamId = mockTeam.Id;
        unrosteredPlayer.State = PlayerState.Unrostered;
        await AddAsync(unrosteredPlayer);

        var unsignedPlayer = CreateFakePlayer();
        unsignedPlayer.TeamId = mockTeam.Id;
        unsignedPlayer.State = PlayerState.Unsigned;
        await AddAsync(unsignedPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<TeamListResult>(TeamListRouteFactory.Uri);

        result.Should().NotBeNull();
        result!.Teams.Should().Contain(t => t.RosteredPlayerCount == "1"
            && t.UnsignedPlayerCount == "1"
            && t.UnrosteredPlayerCount == "1");
    }
}
