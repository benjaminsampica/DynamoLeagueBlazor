namespace DynamoLeagueBlazor.Tests.Features.Teams;

internal class ListTests : IntegrationTestBase
{
    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var client = CreateUnauthenticatedClient(options =>
        {
            options.AllowAutoRedirect = false;
        });

        var response = await client.GetAsync("/teams/list");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
