using Coravel.Invocable;
using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

public class ExpireOfferMatchingService : IInvocable
{
    private readonly ApplicationDbContext _dbContext;
    private ILogger<ExpireOfferMatchingService> _logger;

    public ExpireOfferMatchingService(ApplicationDbContext dbContext, ILogger<ExpireOfferMatchingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var players = await _dbContext.Players
            .AsTracking()
            .Include(p => p.Bids)
            .Where(p => p.State == PlayerState.OfferMatching
                && p.EndOfFreeAgency!.Value.AddDays(3) <= DateTime.Today)
            .ToListAsync();

        if (!players.Any()) return;

        _logger.LogInformation($"Expiring offer matching for {players.Count} players.");

        foreach (var player in players)
        {
            if (!player.Bids.Any())
            {
                _dbContext.Remove(player);

                _logger.LogInformation($"Deleted {player.Name} from the league.");
            }
            else
            {
                player.ExpireMatch();
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
