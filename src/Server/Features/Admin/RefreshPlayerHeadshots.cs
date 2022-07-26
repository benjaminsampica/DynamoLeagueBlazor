using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;

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
    private readonly IPlayerHeadshotService _playerHeadshotService;

    public RefreshPlayerHeadshotsHandler(ApplicationDbContext dbContext, IPlayerHeadshotService playerHeadshotService)
    {
        _dbContext = dbContext;
        _playerHeadshotService = playerHeadshotService;
    }

    public async Task<Unit> Handle(RefreshPlayerHeadshotsCommand request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players.AsTracking().ToListAsync(cancellationToken);
        foreach (var player in players)
        {
            string? headshot = await _playerHeadshotService.FindPlayerHeadshotUrlAsync(player.Name, player.Position, cancellationToken);
            player.HeadShotUrl = headshot!;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}