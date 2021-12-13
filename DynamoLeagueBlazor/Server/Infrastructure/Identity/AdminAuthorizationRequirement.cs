using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

internal sealed class AdminAuthorizationRequirement : IAuthorizationRequirement
{
    public const string Name = "Admin";
}

internal sealed class DynamoLeagueAdminAuthorizationHandler : AuthorizationHandler<AdminAuthorizationRequirement>
{
    private readonly string[] _adminNames = new string[] { "bearsss111@gmail.com", "lyddon.morg@gmail.com", "benjamin.sampica@gmail.com" };

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminAuthorizationRequirement requirement)
    {
        var userName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (_adminNames.Contains(userName))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
