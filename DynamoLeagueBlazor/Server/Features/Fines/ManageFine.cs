using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Fines;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize(Policy = PolicyRequirements.Admin)]
[Route(ManageFineRouteFactory.Uri)]
[ApiController]
public class ManageFineController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ManageFineController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ManageFineRequest request, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<ManageFineCommand>(request);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}

public record ManageFineCommand(int FineId, bool Approved) : IRequest { }

public class ManageFineHandler : IRequestHandler<ManageFineCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public ManageFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ManageFineCommand request, CancellationToken cancellationToken)
    {
        var fine = await _dbContext.Fines
            .AsTracking()
            .SingleAsync(f => f.Id == request.FineId, cancellationToken);

        if (!request.Approved) _dbContext.Fines.Remove(fine!);
        else fine!.Status = request.Approved;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ManageFineMappingProfile : Profile
{
    public ManageFineMappingProfile()
    {
        CreateMap<ManageFineRequest, ManageFineCommand>();
    }
}