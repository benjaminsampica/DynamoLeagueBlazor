using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines.List;

[Authorize]
[ApiController]
[Route("fines")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<GetFineListResult> GetAsync()
    {
        return await _mediator.Send(new Query());
    }
}

public class Query : IRequest<GetFineListResult> { }

public class Handler : IRequestHandler<Query, GetFineListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public Handler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetFineListResult> Handle(Query request, CancellationToken cancellationToken)
    {
        var fines = await _dbContext.Fines
            .Include(p => p.Player)
            .AsNoTracking()
            .ProjectTo<GetFineListResult.FineItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new GetFineListResult
        {
            Fines = fines
        };
    }
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Fine, GetFineListResult.FineItem>()
            .ForMember(d => d.FineStatus, mo => mo.MapFrom(s => s.Status ? "Approved" : "Pending"))
            .ForMember(d => d.PlayerName, mo => mo.MapFrom(s => s.Player.Name))
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.Player.HeadShot))
            .ForMember(d => d.FineAmount, mo => mo.MapFrom(s => s.FineAmount.ToString("C2")));
    }
}
