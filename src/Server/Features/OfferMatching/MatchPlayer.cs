using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

[ApiController]
[Route(MatchPlayerRouteFactory.Uri)]
public class MatchPlayerController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchPlayerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<Unit> PostAsync([FromBody] MatchPlayerRequest request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new MatchPlayerCommand(request.PlayerId), cancellationToken);
    }
}

public record MatchPlayerCommand(int PlayerId) : IRequest { }

public class MatchPlayerHandler : IRequestHandler<MatchPlayerCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public MatchPlayerHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(MatchPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players
            .AsTracking()
            .Include(p => p.Bids)
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken));

        player.MatchOffer();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class MatchPlayerValidator : IMatchPlayerValidator
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _dbContext;

    public MatchPlayerValidator(ICurrentUserService currentUserService, ApplicationDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    public async Task<bool> CanOfferMatchAsync(int playerId, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _currentUserService.GetTeamId();

        var canOfferMatch = await _dbContext.Players
            .AnyAsync(p => p.TeamId == currentUserTeamId
                && p.Id == playerId, cancellationToken);

        return canOfferMatch;
    }
}
