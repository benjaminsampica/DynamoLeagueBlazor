using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Route(AddPlayerFineRouteFactory.Uri)]
[ApiController]
public class AddPlayerFineController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddPlayerFineController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddPlayerFineRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddPlayerFineCommand>(request);

        return await _mediator.Send(query, cancellationToken);
    }
}

public record AddPlayerFineCommand(int PlayerId, string FineReason) : IRequest<int> { }

public class AddPlayerFineHandler : IRequestHandler<AddPlayerFineCommand, int>
{
    private readonly ApplicationDbContext _dbContext;

    public AddPlayerFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(AddPlayerFineCommand request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players
            .AsTracking()
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken));

        var amount = FineUtilities.CalculateFineAmount(player!.ContractValue);

        var fine = player.AddFine(amount, request.FineReason);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return fine.Id;
    }
}

public class AddPlayerFineMappingProfile : Profile
{
    public AddPlayerFineMappingProfile()
    {
        CreateMap<AddPlayerFineRequest, AddPlayerFineCommand>();
    }
}