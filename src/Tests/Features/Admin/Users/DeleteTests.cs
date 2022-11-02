using DynamoLeagueBlazor.Client.Features.Admin.Users;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DynamoLeagueBlazor.Tests.Features.Admin.Users;

public class DeleteTests : IntegrationTestBase
{
    private static DeleteUserRequest CreateFakeValidRequest()
    {
        return new DeleteUserRequest { UserId = RandomString };

    }

    [Fact]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.Create(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = GetUserAuthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.Create(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenThereIsOneUser_ThenDeletesTheUser()
    {
        var application = GetAdminAuthenticatedApplication();

        var userManager = application.Services.GetRequiredService<UserManager<ApplicationUser>>();

        var stubTeam = CreateFakeTeam();
        await AddAsync(stubTeam);
        var mockUser = CreateFakeUser(stubTeam.Id);
        await userManager.CreateAsync(mockUser);
        var request = new DeleteUserRequest { UserId = mockUser.Id };
        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.Create(request);
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        userManager.Users.Should().BeEmpty();
    }
}
