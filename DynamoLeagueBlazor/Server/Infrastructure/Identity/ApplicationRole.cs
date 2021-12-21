using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

