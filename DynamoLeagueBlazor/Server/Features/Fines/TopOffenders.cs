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
[Route("fines/topoffenders")]
public class TopOffendersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TopOffendersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<GetTopOffendersResult> GetAsync()
    {
        return await _mediator.Send(new TopOffendersQuery());
    }
}

public class TopOffendersQuery : IRequest<GetTopOffendersResult> { }

public class TopOffendersHandler : IRequestHandler<TopOffendersQuery, GetTopOffendersResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public TopOffendersHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetTopOffendersResult> Handle(TopOffendersQuery request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Where(p => p.Fines.Any(f => f.Status))
            .OrderByDescending(p => p.Fines.Sum(f => f.FineAmount))
            .Take(10)
            .ProjectTo<GetTopOffendersResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new GetTopOffendersResult
        {
            Players = players
        };
    }
}

public class TopOffendersMappingProfile : Profile
{
    public TopOffendersMappingProfile()
    {
        CreateMap<Player, GetTopOffendersResult.PlayerItem>()
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.HeadShot))
            .ForMember(d => d.PlayerName, mo => mo.MapFrom(s => s.Name))
            .ForMember(d => d.TotalFineAmount, mo => mo.MapFrom(s => s.Fines.Sum(f => f.FineAmount).ToString("C0")));
    }
}
