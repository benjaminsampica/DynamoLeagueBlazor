using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamListResult;

namespace DynamoLeagueBlazor.Server.Features.Teams;

[Authorize]
[ApiController]
[Route("teams")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<TeamListResult> GetAsync()
    {
        return await _mediator.Send(new ListQuery());
    }
}

public class ListQuery : IRequest<TeamListResult> { }

public class ListHandler : IRequestHandler<ListQuery, TeamListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
            .Include(t => t.Players)
            .ProjectTo<TeamItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new TeamListResult
        {
            Teams = teams
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Team, TeamItem>()
            .ForMember(p => p.RosteredPlayerCount, mo => mo.MapFrom(t => t.Players.Count))
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace().ToString("C0")));
    }
}
