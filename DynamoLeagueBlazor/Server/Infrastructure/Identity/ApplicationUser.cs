using DynamoLeagueBlazor.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser(string userName, int teamId) : base(userName)
    {
        Email = userName;
        TeamId = teamId;
    }

    public int TeamId { get; set; }

    public Team Team { get; set; } = null!;
}
