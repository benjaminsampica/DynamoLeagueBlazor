using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
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

        await AddFakeUserAsync("test@gmail.com", 1);
        await AddFakeUserAsync("test2@gmail.com", 2);
    }

    private async Task AddFakeUserAsync(string email, int teamId)
    {
        if (await _userManager.FindByEmailAsync(email) is null)
        {
            var user = new ApplicationUser(email, teamId) { EmailConfirmed = true, Approved = true };
            await _userManager.CreateAsync(user, "hunter2");

            await _userManager.AddToRoleAsync(user, RoleName.Admin);
        }
    }

    private async Task SeedApplicationDataAsync(CancellationToken cancellationToken)
    {
        var baseImage = "https://yahoofantasysports-res.cloudinary.com/image/upload/t_s192sq/fantasy-logos/57182575954_a32e35.jpg";

        if (!await _dbContext.Teams.AnyAsync(cancellationToken))
        {

            var teams = new List<Team>
            {
                new Team("Space Force", baseImage),
                new Team("The Donald", baseImage),
                new Team("Big Chief no Fart", baseImage),
                new Team("Altoona Tunafish", baseImage),
                new Team("Can't Fine This", baseImage),
                new Team("Finkle Einhorn", baseImage),
                new Team("J.J. Mafia", baseImage),
                new Team("Natty Lite", baseImage),
                new Team("Starts With a W", baseImage),
                new Team("Magic SKOL Bus", baseImage)
            };
            _dbContext.Teams.AddRange(teams);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await _dbContext.Players.AnyAsync(cancellationToken))
        {
            var random = new Random();
            for (int i = 1; i < 250; i++)
            {
                var randomTeamId = random.Next(1, 10);
                var player = new Player
                {
                    Name = "Atlanta",
                    Position = "DEF",
                    TeamId = randomTeamId,
                    HeadShotUrl = baseImage
                };

                if (i % 2 == 0)
                {
                    player.SignForCurrentTeam(DateTime.Today.AddYears(1).Year, i);

                    if (i % 4 == 0)
                    {
                        player.BeginNewSeason(DateTime.Today.AddYears(1));
                    }

                    if (i % 8 == 0)
                    {
                        player.EndBidding();
                    }

                    if (i % 16 == 0)
                    {
                        player.AddBid(new Random().Next(), randomTeamId);
                        player.MatchOffer();
                    }
                }

                _dbContext.Players.Add(player);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
