using AutoBogus;
using DynamoLeagueBlazor.Server.Features.Admin;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Admin;
using Microsoft.EntityFrameworkCore;
using MockHttp;
using static DynamoLeagueBlazor.Server.Features.Admin.RefreshPlayerHeadshotsHandler;

namespace DynamoLeagueBlazor.Tests.Features.Fines;

public class RefreshPlayerHeadshotsTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var response = await client.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WhenAPlayerIsMatchedOnNameAndPosition_ThenThePlayerHeadshotIsUpdated()
    {
        var mockHttpHandler = new MockHttpHandler();
        var httpClient = new HttpClient(mockHttpHandler)
        {
            BaseAddress = new Uri(PlayerProfilerUri)
        };

        var stubPlayerDataResult = new AutoFaker<PlayerDataResult>().Generate();
        mockHttpHandler.When(HttpMethod.Get, $"{PlayerProfilerUri}/players")
            .RespondsWithJson(stubPlayerDataResult);

        var matchingPlayer = stubPlayerDataResult.Data.Players.First();

        var stubPlayerMetricDataResult = new AutoFaker<PlayerMetricDataResult>().Generate();
        stubPlayerMetricDataResult.Data.Player.PlayerId = matchingPlayer.PlayerId;
        mockHttpHandler.When(HttpMethod.Get, $"{PlayerProfilerUri}/player/{matchingPlayer.PlayerId}")
            .RespondsWithJson(stubPlayerMetricDataResult);

        var dbContext = GetRequiredService<ApplicationDbContext>();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.Name = matchingPlayer.FullName;
        mockPlayer.Position = matchingPlayer.Position;
        dbContext.Players.Add(mockPlayer);
        await dbContext.SaveChangesAsync();

        var sut = new RefreshPlayerHeadshotsHandler(dbContext, httpClient);

        await sut.Handle(new RefreshPlayerHeadshotsCommand(), CancellationToken.None);

        var expectedPlayer = await dbContext.Players.FirstAsync();
        expectedPlayer.HeadShotUrl.Should().Be(stubPlayerMetricDataResult.Data.Player.Core.Avatar);
    }

    [Fact]
    public async Task WhenAPlayerIsNotMatchedOnName_ThenTheHeadshotRemainsUnchanged()
    {
        var mockHttpHandler = new MockHttpHandler();
        var httpClient = new HttpClient(mockHttpHandler)
        {
            BaseAddress = new Uri(PlayerProfilerUri)
        };

        var stubPlayerDataResult = new AutoFaker<PlayerDataResult>().Generate();
        mockHttpHandler.When(HttpMethod.Get, $"{PlayerProfilerUri}/players")
            .RespondsWithJson(stubPlayerDataResult);

        var matchingPlayer = stubPlayerDataResult.Data.Players.First();

        var dbContext = GetRequiredService<ApplicationDbContext>();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.Position = matchingPlayer.Position;
        dbContext.Players.Add(mockPlayer);
        await dbContext.SaveChangesAsync();

        var sut = new RefreshPlayerHeadshotsHandler(dbContext, httpClient);

        await sut.Handle(new RefreshPlayerHeadshotsCommand(), CancellationToken.None);

        var expectedPlayer = await dbContext.Players.FirstAsync();
        expectedPlayer.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
    }

    [Fact]
    public async Task WhenAPlayerIsNotMatchedOnPosition_ThenTheHeadshotRemainsUnchanged()
    {
        var mockHttpHandler = new MockHttpHandler();
        var httpClient = new HttpClient(mockHttpHandler)
        {
            BaseAddress = new Uri(PlayerProfilerUri)
        };

        var stubPlayerDataResult = new AutoFaker<PlayerDataResult>().Generate();
        mockHttpHandler.When(HttpMethod.Get, $"{PlayerProfilerUri}/players")
            .RespondsWithJson(stubPlayerDataResult);

        var matchingPlayer = stubPlayerDataResult.Data.Players.First();

        var dbContext = GetRequiredService<ApplicationDbContext>();

        var mockPlayer = CreateFakePlayer();
        mockPlayer.Name = matchingPlayer.FullName;
        dbContext.Players.Add(mockPlayer);
        await dbContext.SaveChangesAsync();

        var sut = new RefreshPlayerHeadshotsHandler(dbContext, httpClient);

        await sut.Handle(new RefreshPlayerHeadshotsCommand(), CancellationToken.None);

        var expectedPlayer = await dbContext.Players.FirstAsync();
        expectedPlayer.HeadShotUrl.Should().Be(mockPlayer.HeadShotUrl);
    }
}
