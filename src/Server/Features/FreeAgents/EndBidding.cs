using Coravel.Invocable;
using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

public class EndBiddingService : IInvocable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<EndBiddingService> _logger;

    public EndBiddingService(ApplicationDbContext dbContext, ILogger<EndBiddingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var players = await _dbContext.Players
            .AsTracking()
            .Where(p => p.State == PlayerState.FreeAgent
                && p.EndOfFreeAgency <= DateTime.Now)
            .ToListAsync();

        if (!players.Any()) return;

        _logger.LogInformation("Ending bidding for {playerCount} players.", players.Count);

        foreach (var player in players)
        {
            player.EndBidding();
        }

        await _dbContext.SaveChangesAsync();
    }
}
