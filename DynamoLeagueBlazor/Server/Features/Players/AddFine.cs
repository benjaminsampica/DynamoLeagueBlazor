using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<int> PostAsync([FromBody] AddFineRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddFineCommand>(request);

        return await _mediator.Send(query, cancellationToken);
    }
}

public record AddFineCommand(int PlayerId, string FineReason) : IRequest<int> { }

public class AddFineHandler : IRequestHandler<AddFineCommand, int>
{
    private readonly ApplicationDbContext _dbContext;

    public AddFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(AddFineCommand request, CancellationToken cancellationToken)
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

public class AddFineMappingProfile : Profile
{
    public AddFineMappingProfile()
    {
        CreateMap<AddFineRequest, AddFineCommand>();
    }
}