using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        await SeedApplicationDataAsync(cancellationToken);

        await SeedIdentityDataAsync();

        return Unit.Value;
    }

    private async Task SeedIdentityDataAsync()
    {
        if (!await _roleManager.RoleExistsAsync(RoleName.Admin))
        {
            var adminRole = new ApplicationRole(RoleName.Admin);
            await _roleManager.CreateAsync(adminRole);
        }
        if (!await _roleManager.RoleExistsAsync(RoleName.User))
        {
            var userRole = new ApplicationRole(RoleName.User);
            await _roleManager.CreateAsync(userRole);
        }

        if (await _userManager.FindByEmailAsync("test@gmail.com") is null)
        {
            var user = new ApplicationUser("test@gmail.com", 1) { EmailConfirmed = true, Approved = true };
            await _userManager.CreateAsync(user, "hunter2");

            await _userManager.AddToRoleAsync(user, RoleName.Admin);
        }
    }

    private async Task SeedApplicationDataAsync(CancellationToken cancellationToken)
    {
        if (!await _dbContext.Teams.AnyAsync(cancellationToken))
        {
            var teams = new List<Team>
            {
                new Team("Space Force", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("The Donald", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Big Chief no Fart", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Altoona Tunafish", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Can't Fine This", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Finkle Einhorn", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("J.J. Mafia", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Natty Lite", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Starts With a W", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg"),
                new Team("Magic SKOL Bus", "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg")
            };
            _dbContext.Teams.AddRange(teams);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await _dbContext.Players.AnyAsync(cancellationToken))
        {
            for (int i = 1; i < 250; i++)
            {
                var player = new Player("Atlanta", "DEF")
                {
                    TeamId = new Random().Next(1, 10)
                };

                if (i % 2 == 0)
                {
                    player.SetToRostered(DateTime.Today.AddYears(1).Year, 1);

                    if (i % 10 == 0)
                    {
                        player.SetToUnrostered();
                    }
                }
                else if (i % 5 == 0)
                {
                    player.SetToUnsigned();
                }
                else
                {
                    player.SetToRostered(DateTime.Today.AddYears(-1).Year, 1);
                    if (i % 7 == 0)
                    {
                        player.SetToFreeAgent(DateTime.Today.AddYears(1));
                    }
                    else
                    {
                        player.SetToFreeAgent(DateTime.Today.AddDays(-2));
                        player.TeamId = 1;
                    }
                }

                _dbContext.Players.Add(player);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
