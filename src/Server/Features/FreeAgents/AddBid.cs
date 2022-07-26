using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Route(AddBidRouteFactory.Uri)]
[ApiController]
public class AddBidController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBidValidator _bidValidator;

    public AddBidController(IMediator mediator, IMapper mapper, IBidValidator bidValidator)
    {
        _mediator = mediator;
        _mapper = mapper;
        _bidValidator = bidValidator;
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
        var hasBiddingEnded = await _bidValidator.HasNotEndedAsync(new AddBidRequest { Amount = amount, PlayerId = playerId }, cancellationToken);

        return hasBiddingEnded;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddBidRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddBidCommand>(request);

        return await _mediator.Send(query, cancellationToken);
    }
}

public record AddBidCommand(int PlayerId, int Amount) : IRequest<int> { }

public class AddBidHandler : IRequestHandler<AddBidCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddBidHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> Handle(AddBidCommand request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players
            .AsTracking()
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken);

        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();
        var bid = player!.AddBid(request.Amount, currentUserTeamId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return bid.Id;
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

    public BidValidator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasNotEndedAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players.FindAsync(new object[] { request.PlayerId }, cancellationToken);

        var hasNotEnded = player!.EndOfFreeAgency > DateTime.Now;

        return hasNotEnded;
    }

    public async Task<bool> IsHighestAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var isHighestBid = await _dbContext.Players
            .Where(p => p.Id == request.PlayerId
                && p.Bids.All(b => request.Amount > b.Amount))
            .AnyAsync(cancellationToken);

        return isHighestBid;
    }
}
