using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using static DynamoLeagueBlazor.Shared.Features.Admin.TeamNameListResult;

namespace DynamoLeagueBlazor.Server.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(AddPlayerRouteFactory.Uri)]
public class AddPlayerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddPlayerController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddPlayerRequest request, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<AddPlayerCommand>(request);

        return await _mediator.Send(command, cancellationToken);
    }

    [HttpGet("teams")]
    public async Task<TeamNameListResult> GetTeamsAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new TeamListQuery(), cancellationToken);
    }

    [HttpGet("playerpreview")]
    public async Task<PlayerPreviewResponse> GetPlayerHeadshotUrlAsync([FromQuery] PlayerPreviewRequest request, CancellationToken cancellationToken)
    {
        var findPlayerPreviewRequest = _mapper.Map<FindPlayerPreviewRequest>(request);

        return await _mediator.Send(findPlayerPreviewRequest, cancellationToken);
    }
}

public record AddPlayerCommand(string Name, string Position, int TeamId, int ContractValue) : IRequest<int> { }

public class AddPlayerHandler : IRequestHandler<AddPlayerCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPlayerHeadshotService _playerHeadshotService;

    public AddPlayerHandler(ApplicationDbContext dbContext, IPlayerHeadshotService playerHeadshotService)
    {
        _dbContext = dbContext;
        _playerHeadshotService = playerHeadshotService;
    }

    public async Task<int> Handle(AddPlayerCommand request, CancellationToken cancellationToken)
    {
        var headshotUrl = await _playerHeadshotService.FindPlayerHeadshotUrlAsync(request.Name, request.Position, cancellationToken);

        var player = new Player(request.Name, request.Position, headshotUrl!)
        {
            ContractValue = request.ContractValue,
            TeamId = request.TeamId
        };

        _dbContext.Add(player);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return player.Id;
    }
}

public class AddPlayerMappingProfile : Profile
{
    public AddPlayerMappingProfile()
    {
        CreateMap<AddPlayerRequest, AddPlayerCommand>();
    }
}

public record TeamListQuery : IRequest<TeamNameListResult> { }

public class TeamListHandler : IRequestHandler<TeamListQuery, TeamNameListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public TeamListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamNameListResult> Handle(TeamListQuery request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
                .ProjectTo<TeamNameItem>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        return new TeamNameListResult
        {
            Teams = teams
        };
    }
}

public class TeamListMappingProfile : Profile
{
    public TeamListMappingProfile()
    {
        CreateMap<Team, TeamNameItem>();
    }
}

public record FindPlayerPreviewRequest(string Name, string Position) : IRequest<PlayerPreviewResponse> { }

public class FindPlayerPreviewRequestHandler : IRequestHandler<FindPlayerPreviewRequest, PlayerPreviewResponse>
{
    private readonly IPlayerHeadshotService _playerHeadshotService;

    public FindPlayerPreviewRequestHandler(IPlayerHeadshotService playerHeadshotService)
    {
        _playerHeadshotService = playerHeadshotService;
    }

    public async Task<PlayerPreviewResponse> Handle(FindPlayerPreviewRequest request, CancellationToken cancellationToken)
    {
        var headshotUrl = await _playerHeadshotService.FindPlayerHeadshotUrlAsync(request.Name, request.Position, cancellationToken);

        return new PlayerPreviewResponse
        {
            HeadshotUrl = headshotUrl
        };
    }
}

public class FindPlayerPreviewMappingProfile : Profile
{
    public FindPlayerPreviewMappingProfile()
    {
        CreateMap<PlayerPreviewRequest, FindPlayerPreviewRequest>();
    }
}