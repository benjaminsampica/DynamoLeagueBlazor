using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Shared.Features.Fines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize]
[Route("fines/manage")]
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
    public async Task<IActionResult> PostAsync([FromBody] ManageFineRequest request)
    {
        var query = _mapper.Map<ManageFineQuery>(request);

        await _mediator.Send(query);

        return NoContent();
    }
}

public record ManageFineQuery(int FineId, bool Approved) : IRequest { }

public class ManageFineHandler : IRequestHandler<ManageFineQuery>
{
    private readonly ApplicationDbContext _dbContext;

    public ManageFineHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ManageFineQuery request, CancellationToken cancellationToken)
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
        CreateMap<ManageFineRequest, ManageFineQuery>();
    }
}