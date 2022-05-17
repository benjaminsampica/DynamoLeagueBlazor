using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[ApiController]
[Route(TopOffendersRouteFactory.Uri)]
public class TopOffendersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TopOffendersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<TopOffendersResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new TopOffendersQuery(), cancellationToken);
    }
}

public class TopOffendersQuery : IRequest<TopOffendersResult> { }

public class TopOffendersHandler : IRequestHandler<TopOffendersQuery, TopOffendersResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public TopOffendersHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TopOffendersResult> Handle(TopOffendersQuery request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Where(p => p.Fines.Any(f => f.Status))
            .OrderByDescending(p => p.Fines.Sum(f => f.Amount))
            .Take(10)
            .ProjectTo<TopOffendersResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new TopOffendersResult
        {
            Players = players
        };
    }
}

public class TopOffendersMappingProfile : Profile
{
    public TopOffendersMappingProfile()
    {
        CreateMap<Player, TopOffendersResult.PlayerItem>()
            .ForMember(d => d.TotalFineAmount, mo => mo.MapFrom(s => s.Fines.Sum(f => f.Amount).ToString("C0")));
    }
}
