using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        return await _mediator.Send(new ListQuery());
    }
}

public class ListQuery : IRequest<GetTeamListResult> { }

public class ListHandler : IRequestHandler<ListQuery, GetTeamListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetTeamListResult> Handle(ListQuery request, CancellationToken cancellationToken)
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

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Team, GetTeamListResult.TeamItem>()
            .ForMember(p => p.PlayerCount, mo => mo.MapFrom(t => t.Players.Count))
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace().ToString("C0")));
    }
}
