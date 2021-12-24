using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize]
[Route("players/addfine")]
[ApiController]
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
    public async Task<int> PostAsync([FromBody] AddFineRequest request)
    {
        var query = _mapper.Map<AddFineQuery>(request);

        return await _mediator.Send(query);
    }
}

public record AddFineQuery(int PlayerId, string FineReason) : IRequest<int> { }

public class AddFineHandler : IRequestHandler<AddFineQuery, int>
{
    private readonly ApplicationDbContext _dbContext;

    public AddFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(AddFineQuery request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players.FindAsync(new object?[] { request.PlayerId }, cancellationToken));

        var amount = FineUtilities.CalculateFineAmount(player!.ContractValue);

        var fine = player.AddFine(amount, request.FineReason);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return fine.Id;
    }
}

public class AddFineMappingProfile : Profile
{
    public AddFineMappingProfile()
    {
        CreateMap<AddFineRequest, AddFineQuery>();
    }
}