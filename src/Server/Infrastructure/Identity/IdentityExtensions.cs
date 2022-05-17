using System.Security.Claims;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public static class IdentityExtensions
{
    public static int GetTeamId(this ClaimsPrincipal claimsPrincipal)
        => int.Parse(claimsPrincipal.FindFirst(nameof(ApplicationUser.TeamId))!.Value);
}
