using DynamoLeagueBlazor.Client.Features.Admin.Users;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DynamoLeagueBlazor.Tests.Features.Admin.Users;

internal class DeleteTests
{
    private static DeleteUserRequest CreateFakeValidRequest()
    {
        return new DeleteUserRequest { UserId = RandomString };

    }

    [Test]
    public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenAuthenticatedUser_ThenDoesNotAllowAccess()
    {
        var application = CreateUserAuthenticatedApplication();

        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(CreateFakeValidRequest());
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenAuthenticatedAdmin_WhenThereIsOneUser_ThenDeletesTheUser()
    {
        var application = CreateUserAuthenticatedApplication();

        var userManager = application.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var mockUser = CreateFakeUser(int.MaxValue);
        await userManager.CreateAsync(mockUser);
        await userManager.AddToRoleAsync(mockUser, RoleName.Admin);
        var request = new DeleteUserRequest { UserId = mockUser.Id };
        var client = application.CreateClient();

        var endpoint = DeleteUserRouteFactory.CreateRequestUri(request);
        var response = await client.DeleteAsync(endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await userManager.FindByIdAsync(mockUser.Id);
        user.Should().BeNull();
    }
}
