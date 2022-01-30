using DynamoLeagueBlazor.Client.Features.Admin.Users;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin;
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
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GivenAuthenticatedAdmin_WhenThereIsOneUser_ThenDeletesTheUser()
    {
        var application = CreateAdminAuthenticatedApplication();

        var userManager = application.Services.GetRequiredService<UserManager<ApplicationUser>>();

        var stubTeam = CreateFakeTeam();
        await application.AddAsync(stubTeam);
        var mockUser = CreateFakeUser(stubTeam.Id);
        await userManager.CreateAsync(mockUser);
        var request = new DeleteUserRequest { UserId = mockUser.Id };
        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(request);
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        userManager.Users.Should().BeEmpty();
    }
}
