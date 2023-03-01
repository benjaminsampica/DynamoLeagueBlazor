using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

[ApiController]
[Route(MatchPlayerRouteFactory.Uri)]
public class MatchPlayerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<MatchPlayerRequest> _validator;

    public MatchPlayerController(IMediator mediator, IValidator<MatchPlayerRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([CustomizeValidator(Skip = true)][FromBody] MatchPlayerRequest request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        await _mediator.Send(new MatchPlayerCommand(request.PlayerId), cancellationToken);

        return Ok();
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

    public async Task Handle(MatchPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players
            .AsTracking()
            .Include(p => p.Bids)
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken));

        player.MatchOffer();

        await _dbContext.SaveChangesAsync(cancellationToken);
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
