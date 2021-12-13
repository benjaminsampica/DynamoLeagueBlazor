using DynamoLeagueBlazor.Server.Areas.Identity;
using DynamoLeagueBlazor.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Infrastructure;

public class SeedDataCommand : IRequest { }

public class Handler : IRequestHandler<SeedDataCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public Handler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
    {
        await SeedIdentityDataAsync();

        await SeedTeamDataAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task SeedIdentityDataAsync()
    {
        var adminRole = new ApplicationRole(ApplicationRole.Admin);
        await _roleManager.CreateAsync(adminRole);

        var userRole = new ApplicationRole(ApplicationRole.User);
        await _roleManager.CreateAsync(userRole);

        var user = new ApplicationUser("benjamin.sampica@gmail.com", 1) { EmailConfirmed = true };
        await _userManager.CreateAsync(user, "hunter2");

        await _userManager.AddToRoleAsync(user, ApplicationRole.Admin);
    }

    private async Task SeedTeamDataAsync(CancellationToken cancellationToken)
    {
        var teams = new List<Team>
        {
            new Team("390.l.40360.t.1", "Space Force", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.2", "The Donald", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.3", "Big Chief no Fart", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.4", "Altoona Tunafish", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.5", "Can't Fine This", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.6", "Finkle Einhorn", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.7", "J.J. Mafia", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.8", "Natty Lite", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.9", "Starts With a W", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
            new Team("390.l.40360.t.10", "Magic SKOL Bus", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg")
        };
        _dbContext.Teams.AddRange(teams);
        await _dbContext.SaveChangesAsync(cancellationToken);

        for (int i = 1; i < 250; i++)
        {
            var player = new Player("390.p.100001", "Atlanta", "DEF", "https://s.yimg.com/lq/i/us/sp/v/nfl/teams/1/50x50w/chi.gif")
            {
                TeamId = new Random().Next(1, 10)
            };

            if (i % 2 == 0)
            {
                player.SetToRostered(DateTime.Today.AddYears(1), 1);

                if (i % 10 == 0)
                {
                    player.SetToUnrostered();
                }
            }
            else
            {
                player.SetToUnsigned();
            }

            _dbContext.Players.Add(player);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
