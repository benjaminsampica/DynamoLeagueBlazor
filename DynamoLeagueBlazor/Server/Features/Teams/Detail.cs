using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Teams;

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

public class Query : IRequest<GetTeamDetailResult> { }

public class Handler : IRequestHandler<Query, GetTeamDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public Handler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetTeamDetailResult> Handle(Query request, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.Teams
            .Include(t => t.Players)
            .AsNoTracking()
            .ProjectTo<GetTeamDetailResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new GetTeamDetailResult
        {
            Teams = teams
        };
    }
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Team, GetTeamDetailResult.PlayerItem>()
            .ForMember(p => p.PlayerCount, mo => mo.MapFrom(t => t.Players.Count))
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace().ToString("C0")));
    }
}
