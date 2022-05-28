using DynamoLeagueBlazor.Server.Features.Admin.Shared;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(RefreshPlayerHeadshotsRouteFactory.Uri)]
public class RefreshPlayerHeadshotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RefreshPlayerHeadshotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RefreshPlayerHeadshotsCommand(), cancellationToken);

        return NoContent();
    }
}

public record RefreshPlayerHeadshotsCommand : IRequest<Unit> { }

public class RefreshPlayerHeadshotsHandler : IRequestHandler<RefreshPlayerHeadshotsCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public RefreshPlayerHeadshotsHandler(ApplicationDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    public async Task<Unit> Handle(RefreshPlayerHeadshotsCommand request, CancellationToken cancellationToken)
    {
        var playerResult = await _httpClient.GetFromJsonAsync<PlayerDataResult>(PlayerProfilerRouteFactory.GetPlayersUri, cancellationToken: cancellationToken);

        if (playerResult != null)
        {
            foreach (var player in playerResult.Data.Players)
            {
                var matchingDynamoLeaguePlayer = await _dbContext.Players
                    .AsTracking()
                    .SingleOrDefaultAsync(p =>
                        p.Name == player.FullName
                        && p.Position == player.Position,
                        cancellationToken);

                if (matchingDynamoLeaguePlayer is null) continue;

                var metricResult = await _httpClient.GetFromJsonAsync<PlayerMetricDataResult>(PlayerProfilerRouteFactory.GetPlayerUri(player.PlayerId), cancellationToken: cancellationToken);

                matchingDynamoLeaguePlayer.HeadShotUrl = metricResult!.Data.Player.Core.Avatar;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}