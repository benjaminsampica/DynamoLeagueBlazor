using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using IdentityModel;

namespace DynamoLeagueBlazor.Server.Areas.Identity;

public class ProfileService : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var nameClaim = context.Subject.FindAll(JwtClaimTypes.Name);
        context.IssuedClaims.AddRange(nameClaim);

        var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
        context.IssuedClaims.AddRange(roleClaims);

        var teamIdClaim = context.Subject.FindFirst(nameof(ApplicationUser.TeamId));
        context.IssuedClaims.Add(teamIdClaim);

        var approvedClaim = context.Subject.FindFirst(nameof(ApplicationUser.Approved));
        context.IssuedClaims.Add(approvedClaim);

        await Task.CompletedTask;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        await Task.CompletedTask;
    }
}