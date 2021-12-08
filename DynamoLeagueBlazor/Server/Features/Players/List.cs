using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Players.List;

[Authorize]
[ApiController]
[Route("Players/[controller]")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<GetPlayerListResult> GetAsync()
    {
        return await _mediator.Send(new Query());
    }
}

public class Query : IRequest<GetPlayerListResult> { }

public class Handler : IRequestHandler<Query, GetPlayerListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public Handler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetPlayerListResult> Handle(Query request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Include(p => p.Team)
            .AsNoTracking()
            .ProjectTo<GetPlayerListResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new GetPlayerListResult
        {
            Players = players
        };
    }
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Player, GetPlayerListResult.PlayerItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team != null ? s.Team.TeamName : string.Empty))
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")));
    }
}
