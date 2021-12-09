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
[Route("Teams/")]
public class DetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public DetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{teamId}")]
    public async Task<GetTeamDetailResult> GetAsync(int teamId)
    {
        return await _mediator.Send(new DetailQuery(teamId));
    }
}

public record DetailQuery(int TeamId) : IRequest<GetTeamDetailResult> { }

public class DetailHandler : IRequestHandler<DetailQuery, GetTeamDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public DetailHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<GetTeamDetailResult> Handle(DetailQuery request, CancellationToken cancellationToken)
    {
        var team = await _dbContext.Teams
            .Where(t => t.Id == request.TeamId)
            .Include(t => t.Players)
            .AsNoTracking()
            .ProjectTo<GetTeamDetailResult>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        return team;
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Player, GetTeamDetailResult.PlayerItem>()
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")));

        CreateMap<Team, GetTeamDetailResult>()
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace().ToString("C0")));
    }
}
