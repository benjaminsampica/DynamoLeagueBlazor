using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(AddTeamFineRouteFactory.Uri)]
public class AddTeamFineController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddTeamFineController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task PostAsync([FromBody] AddTeamFineRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddTeamFineCommand>(request);

        await _mediator.Send(query, cancellationToken);
    }
}

public record AddTeamFineCommand(int TeamId, string FineReason, decimal Amount) : IRequest { }

public class AddTeamFineHandler : IRequestHandler<AddTeamFineCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public AddTeamFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(AddTeamFineCommand request, CancellationToken cancellationToken)
    {
        var team = (await _dbContext.Teams
            .AsTracking()
            .SingleAsync(p => p.Id == request.TeamId, cancellationToken));

        team.AddFine(request.Amount, request.FineReason);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class AddTeamFineMappingProfile : Profile
{
    public AddTeamFineMappingProfile()
    {
        CreateMap<AddTeamFineRequest, AddTeamFineCommand>();
    }
}
