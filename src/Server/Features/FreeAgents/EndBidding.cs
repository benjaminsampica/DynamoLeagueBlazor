using Coravel.Invocable;
using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

public class EndBiddingService : IInvocable
{
    private readonly ApplicationDbContext _dbContext;

    public EndBiddingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Invoke()
    {
        var players = _dbContext.Players
            .AsTracking()
            .Where(p => p.State == PlayerState.FreeAgent
                && p.EndOfFreeAgency <= DateTime.Now);

        foreach (var player in players)
        {
            player.EndBidding();
        }

        await _dbContext.SaveChangesAsync();
    }
}
