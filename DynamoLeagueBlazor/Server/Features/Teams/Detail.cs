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
    public async Task<TeamDetailResult> GetAsync(int teamId)
    {
        return await _mediator.Send(new DetailQuery(teamId));
    }
}

public record DetailQuery(int TeamId) : IRequest<TeamDetailResult> { }

public class DetailHandler : IRequestHandler<DetailQuery, TeamDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public DetailHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TeamDetailResult> Handle(DetailQuery request, CancellationToken cancellationToken)
    {
        var teamWithPlayers = await _dbContext.Teams
            .Include(t => t.Players)
            .Where(t => t.Id == request.TeamId)
            .AsNoTracking()
            .ProjectTo<TeamDetailResult>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        return teamWithPlayers;
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Player, TeamDetailResult.PlayerItem>()
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")));

        CreateMap<Team, TeamDetailResult>()
            .ForMember(p => p.CapSpace, mo => mo.MapFrom(t => t.CapSpace().ToString("C0")))
            .ForMember(p => p.UnrosteredPlayers, mo => mo.MapFrom(s => s.Players))
            .ForMember(p => p.RosteredPlayers, mo => mo.MapFrom(s => s.Players));
    }
}
