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
    public async Task GivenAnyAuthenticatedUser_ThenDoesAllowAccess()
    {
        var application = CreateAuthenticatedApplication();
        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);
        var client = application.CreateClient();
        var endpoint = _endpoint + stubTeam.Id;

        var response = await client.GetAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
