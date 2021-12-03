using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Teams.List;

[Authorize]
[ApiController]
[Route("Teams/[controller]")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<GetTeamListResult> GetAsync()
    {
        return await _mediator.Send(new Query());
    }
}

public class Query : IRequest<GetTeamListResult> { }

public class Handler : IRequestHandler<Query, GetTeamListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public Handler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetTeamListResult> Handle(Query request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
            .Include(t => t.Players)
            .AsNoTracking()
            .ProjectTo<GetTeamListResult.TeamItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new GetTeamListResult
        {
            Teams = teams
        };
    }
}

public class GetTeamListResult
{
    public IEnumerable<TeamItem> Teams { get; init; } = Array.Empty<TeamItem>();

    public record TeamItem(
        string Id,
        string TeamLogoUrl,
        string TeamName,
        int PlayerCount,
        int CapSpace);
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Team, GetTeamListResult.TeamItem>()
            .ForMember(p => p.PlayerCount, mo => mo.MapFrom(t => t.Players.Count))
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace()));
    }
}
