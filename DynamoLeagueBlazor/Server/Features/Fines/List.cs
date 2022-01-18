using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize]
[ApiController]
[Route("api/fines")]
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
            .Include(p => p.Player)
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
            .ForMember(d => d.PlayerName, mo => mo.MapFrom(s => s.Player.Name))
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.Player.HeadShotUrl))
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Amount.ToString("C2")));
    }
}
