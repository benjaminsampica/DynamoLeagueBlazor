using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents.Detail;

[Route(AddBidRouteFactory.Uri)]
[ApiController]
public class AddBidController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBidValidator _bidValidator;
    private readonly IValidator<AddBidRequest> _validator;

    public AddBidController(IMediator mediator, IMapper mapper, IBidValidator bidValidator, IValidator<AddBidRequest> validator)
    {
        _mediator = mediator;
        _mapper = mapper;
        _bidValidator = bidValidator;
        _validator = validator;
    }

    [HttpGet("ishighest")]
    public async Task<bool> GetIsHighestAsync([FromQuery] int playerId, int amount, CancellationToken cancellationToken)
    {
        var isHighestBid = await _bidValidator.IsHighestAsync(new AddBidRequest { Amount = amount, PlayerId = playerId }, cancellationToken);

        return isHighestBid;
    }

    [HttpGet("hasnotended")]
    public async Task<bool> GetHasNotEndedAsync([FromQuery] int playerId, int amount, CancellationToken cancellationToken)
    {
        var hasNotEnded = await _bidValidator.HasNotEndedAsync(new AddBidRequest { Amount = amount, PlayerId = playerId }, cancellationToken);

        return hasNotEnded;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([CustomizeValidator(Skip = true)][FromBody] AddBidRequest request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        var query = _mapper.Map<AddBidCommand>(request);

        await _mediator.Send(query, cancellationToken);

        return Ok();
    }
}

public record AddBidCommand(int PlayerId, int Amount) : IRequest { }

public class AddBidHandler : IRequestHandler<AddBidCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AddBidHandler(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(AddBidCommand request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players
            .Include(p => p.Bids)
            .AsTracking()
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken);

        var currentUserTeamId = _currentUserService.GetTeamId();
        player!.AddBid(request.Amount, currentUserTeamId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class AddBidMappingProfile : Profile
{
    public AddBidMappingProfile()
    {
        CreateMap<AddBidRequest, AddBidCommand>();
    }
}

public class BidValidator : IBidValidator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public BidValidator(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> HasNotEndedAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players.FindAsync(new object[] { request.PlayerId }, cancellationToken);

        var hasNotEnded = player!.EndOfFreeAgency > DateTimeOffset.UtcNow;

        return hasNotEnded;
    }

    public async Task<bool> IsHighestAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _currentUserService.GetTeamId();

        var playerWithBids = await _dbContext.Players.Where(b => b.Id == request.PlayerId)
            .Include(p => p.Bids.Where(pb => pb.Amount >= request.Amount))
            .FirstAsync(cancellationToken);

        var currentHighestBid = playerWithBids.Bids.FindHighestBid();
        if (currentHighestBid != null)
        {
            if (currentHighestBid.TeamId == currentUserTeamId)
            {
                return request.Amount > currentHighestBid.Amount;
            }
            else
            {
                var highestNonOverBid = playerWithBids.GetHighestBidAmount();

                return request.Amount > highestNonOverBid;
            }
        }

        return true;
    }
}
