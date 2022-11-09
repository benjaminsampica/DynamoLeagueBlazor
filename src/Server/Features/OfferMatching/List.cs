using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using static DynamoLeagueBlazor.Server.Models.Player;

namespace DynamoLeagueBlazor.Server.Features.OfferMatching;

[ApiController]
[Route(OfferMatchingListRouteFactory.Uri)]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<OfferMatchingListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}

public record ListQuery : IRequest<OfferMatchingListResult> { }

public class ListHandler : IRequestHandler<ListQuery, OfferMatchingListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListHandler(
        ApplicationDbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OfferMatchingListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();

        var offerMatches = await _dbContext.Players
            .Include(p => p.Bids)
            .ThenInclude(b => b.Team)
            .Where(p => p.State == PlayerState.OfferMatching)
            .ProjectTo<OfferMatchingListResult.OfferMatchingItem>(_mapper.ConfigurationProvider, new { currentUserTeamId })
            .ToListAsync(cancellationToken);

        
        return new OfferMatchingListResult
        {
            OfferMatches = offerMatches
        };
    }
}
public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        int currentUserTeamId = 0;

        CreateMap<Player, OfferMatchingListResult.OfferMatchingItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.OfferingTeam, mo => mo.MapFrom(s => s.GetOfferingTeam()))
            .ForMember(d => d.Offer, mo => mo.MapFrom(s => s.GetHighestBidAmount()))
            .ForMember(d => d.CurrentUserIsOfferMatching, mo => mo.MapFrom(s => s.TeamId == currentUserTeamId))
            .ForMember(d => d.RemainingTime, mo => mo.MapFrom(s => s.GetRemainingFreeAgencyTime()));
    }
}
