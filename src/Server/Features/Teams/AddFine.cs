using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[Authorize(Policy = PolicyRequirements.Admin)]
[ApiController]
[Route(AddFineRouteFactory.Uri)]
public class AddFineController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddFineController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task PostAsync([FromBody] AddFineRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddFineCommand>(request);

        await _mediator.Send(query, cancellationToken);
    }
}

public record AddFineCommand(int TeamId, string FineReason, decimal Amount) : IRequest { }

public class AddFineHandler : IRequestHandler<AddFineCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public AddFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddFineCommand request, CancellationToken cancellationToken)
    {
        var team = (await _dbContext.Teams
            .AsTracking()
            .SingleAsync(p => p.Id == request.TeamId, cancellationToken));

        team.AddFine(request.Amount, request.FineReason);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class AddFineMappingProfile : Profile
{
    public AddFineMappingProfile()
    {
        CreateMap<AddFineRequest, AddFineCommand>();
    }
}
