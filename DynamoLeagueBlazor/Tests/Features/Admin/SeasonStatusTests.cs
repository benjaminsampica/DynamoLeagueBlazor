﻿using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class SeasonStatusTests : IntegrationTestBase
{
    private const string _endpoint = "api/admin/seasonstatus";

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.GetAsync(_endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenAPlayerIsAFreeAgent_ThenReturnsTrue()
    {
        var application = CreateAdminAuthenticatedApplication();
        var mockPlayer = CreateFakePlayer();
        mockPlayer.SetToFreeAgent(DateTime.MaxValue);
        await application.AddAsync(mockPlayer);

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(_endpoint);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenNoPlayerIsAFreeAgent_ThenReturnsFalse()
    {
        var application = CreateAdminAuthenticatedApplication();

        var client = application.CreateClient();

        var result = await client.GetFromJsonAsync<bool>(_endpoint);

        result.Should().BeFalse();
    }
}
