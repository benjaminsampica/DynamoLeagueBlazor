using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Utilities;
using static DynamoLeagueBlazor.Server.Models.Player;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[ApiController]
[Route(TeamDetailRouteFactory.Uri)]
public class DetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public DetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{teamId}")]
    public async Task<TeamDetailResult> GetAsync(int teamId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DetailQuery(teamId), cancellationToken);
    }
}

public record DetailQuery(int TeamId) : IRequest<TeamDetailResult> { }

public class DetailHandler : IRequestHandler<DetailQuery, TeamDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public DetailHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamDetailResult> Handle(DetailQuery request, CancellationToken cancellationToken)
    {
        var teamDetail = await _dbContext.Teams
            .Where(t => t.Id == request.TeamId)
            .ProjectTo<TeamDetailResult>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        var rosteredPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId
                && p.State == PlayerState.Rostered);
        var rosteredPlayers = await rosteredPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.RosteredPlayers.AddRange(rosteredPlayers);

        var rosteredPlayersContractValue = await rosteredPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

        var unrosteredPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId
                && p.State == PlayerState.Unrostered);
        var unrosteredPlayers = await unrosteredPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.UnrosteredPlayers.AddRange(unrosteredPlayers);

        var unrosteredPlayersContractValue = await unrosteredPlayersQuery.SumAsync(urp => urp.ContractValue, cancellationToken);

        var unsignedPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId
                && p.State == PlayerState.Unsigned);
        var unsignedPlayers = await unsignedPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.UnsignedPlayers.AddRange(unsignedPlayers);

        var unsignedPlayersContractValue = await unsignedPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

        teamDetail.CapSpace = CapSpaceUtilities.GetRemainingCapSpace(DateOnly.FromDateTime(DateTime.Today), rosteredPlayersContractValue, unrosteredPlayersContractValue, unsignedPlayersContractValue).ToString("C0");

        return teamDetail;
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Player, PlayerItem>()
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue))
            .ForMember(d => d.YearContractExpires, mo => mo.MapFrom(s => s.YearContractExpires));

        CreateMap<Team, TeamDetailResult>()
            .ForMember(tdr => tdr.RosteredPlayers, mo => mo.Ignore())
            .ForMember(tdr => tdr.UnrosteredPlayers, mo => mo.Ignore())
            .ForMember(tdr => tdr.UnsignedPlayers, mo => mo.Ignore());
    }
}
