using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public class ApplicationUser : IdentityUser, IUser
{
    public ApplicationUser(string userName, int teamId) : base(userName)
    {
        Email = userName;
        TeamId = teamId;
    }

    public int TeamId { get; set; }
    public bool Approved { get; set; }

    public Team Team { get; set; } = null!;
}
