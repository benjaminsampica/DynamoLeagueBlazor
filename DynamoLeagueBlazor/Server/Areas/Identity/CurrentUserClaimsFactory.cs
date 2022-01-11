﻿using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DynamoLeagueBlazor.Server.Areas.Identity;

public class CurrentUserClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public CurrentUserClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim(nameof(user.TeamId), user.TeamId.ToString()));
        identity.AddClaim(new Claim(nameof(user.Approved), user.Approved.ToString()));

        return identity;
    }
}
