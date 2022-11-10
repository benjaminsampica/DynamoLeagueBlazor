using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[ApiController]
[Route(FineListRouteFactory.Uri)]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<FineListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}

public class ListQuery : IRequest<FineListResult> { }

public class ListHandler : IRequestHandler<ListQuery, FineListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<FineListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var fines = await _dbContext.Fines
            .OrderBy(f => f.Status)
            .ProjectTo<FineListResult.FineItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new FineListResult
        {
            Fines = fines
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Fine, FineListResult.FineItem>()
            .ForMember(d => d.Status, mo => mo.MapFrom(s => s.Status ? "Approved" : "Pending"))
            .ForMember(d => d.PlayerName, mo => mo.MapFrom(s => s.Player != null ? s.Player.Name : null))
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.Player != null ? s.Player.HeadShotUrl : null))
            .ForMember(d => d.TeamName, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.TeamLogoUrl, mo => mo.MapFrom(s => s.Team.LogoUrl))
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Amount));
    }
}
