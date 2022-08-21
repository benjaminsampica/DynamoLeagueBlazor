using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

[ApiController]
[Route(FreeAgentDetailFactory.Uri)]
public class DetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public DetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{PlayerId}")]
    public async Task<FreeAgentDetailResult> GetAsync([FromRoute] DetailQuery query, CancellationToken cancellationToken)
    {
        return await _mediator.Send(query, cancellationToken);
    }
}

public record DetailQuery(int PlayerId) : IRequest<FreeAgentDetailResult> { }

public class DetailHandler : IRequestHandler<DetailQuery, FreeAgentDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DetailHandler(ApplicationDbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FreeAgentDetailResult> Handle(DetailQuery request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();

        var result = await _dbContext.Players
            .Include(p => p.Bids)
                .ThenInclude(b => b.Team)
            .Where(p => p.Id == request.PlayerId)
            .ProjectTo<FreeAgentDetailResult>(_mapper.ConfigurationProvider, new { currentUserTeamId })
            .SingleAsync(cancellationToken);

        return result;
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        int currentUserTeamId = 0;

        CreateMap<Player, FreeAgentDetailResult>()
            .ForMember(d => d.EndOfFreeAgency, mo => mo.MapFrom(s => s.EndOfFreeAgency!.Value))
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.Bids, mo => mo.MapFrom(s => s.Bids.Where(b => b.IsOverBid == false).OrderByDescending(b => b.UpdatedOn)))
            .ForMember(d => d.OverBid, mo => mo.MapFrom(s => s.Bids.FirstOrDefault(b => b.IsOverBid && b.TeamId == currentUserTeamId)));
        CreateMap<Bid, FreeAgentDetailResult.BidItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Amount))
            .ForMember(d => d.CreatedOn, mo => mo.MapFrom(s => s.CreatedOn.ToString("g")));
    }
}