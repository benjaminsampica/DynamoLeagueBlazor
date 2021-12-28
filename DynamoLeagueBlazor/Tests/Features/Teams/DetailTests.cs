﻿using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Utilities;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Teams;

internal class DetailTests : IntegrationTestBase
{
    private const string _endpoint = "/teams/";

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();
        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);
        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAnyAuthenticatedUser_WhenGivenValidTeamId_ThenReturnsExpectedResult()
    {
        var application = CreateUserAuthenticatedApplication();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);

        var mockRosteredPlayer = CreateFakePlayer();
        mockRosteredPlayer.TeamId = stubTeam.Id;
        mockRosteredPlayer.SetToRostered(DateTime.MaxValue, 1);
        await application.AddAsync(mockRosteredPlayer);

        var mockUnrosteredPlayer = CreateFakePlayer();
        mockUnrosteredPlayer.TeamId = stubTeam.Id;
        mockUnrosteredPlayer.SetToUnrostered();
        await application.AddAsync(mockUnrosteredPlayer);

        var mockUnsignedPlayer = CreateFakePlayer();
        mockUnsignedPlayer.TeamId = stubTeam.Id;
        mockUnsignedPlayer.SetToUnsigned();
        await application.AddAsync(mockUnsignedPlayer);

        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetFromJsonAsync<TeamDetailResult>(endpoint);

        response.Should().NotBeNull();
        response!.Name.Should().Be(stubTeam.Name);
        response.CapSpace.Should().Be(CapSpaceUtilities.CalculateCurrentCapSpace(DateOnly.FromDateTime(DateTime.Today), mockRosteredPlayer.ContractValue, mockUnrosteredPlayer.ContractValue, mockUnsignedPlayer.ContractValue).ToString("C0"));
        response.RosteredPlayers.Should().NotBeEmpty();
        response.UnrosteredPlayers.Should().NotBeEmpty();
        response.UnsignedPlayers.Should().NotBeEmpty();
    }
}
