using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser(string userName, string email, int teamId) : base(userName)
    {
        TeamId = teamId;
        Email = email;
    }

    public int TeamId { get; set; }
}
