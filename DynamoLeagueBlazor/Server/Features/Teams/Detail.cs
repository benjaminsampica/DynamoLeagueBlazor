using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

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
    public async Task<TeamDetailResult> GetAsync(int teamId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DetailQuery(teamId), cancellationToken);
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
        var teamDetail = await _dbContext.Teams
            .Where(t => t.Id == request.TeamId)
            .ProjectTo<TeamDetailResult>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        var rosteredPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId)
            .WhereIsRostered();
        var rosteredPlayers = await rosteredPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.RosteredPlayers.AddRange(rosteredPlayers);

        var rosteredPlayersContractValue = await rosteredPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

        var unrosteredPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId)
            .WhereIsUnrostered();
        var unrosteredPlayers = await unrosteredPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.UnrosteredPlayers.AddRange(unrosteredPlayers);

        var unrosteredPlayersContractValue = await unrosteredPlayersQuery.SumAsync(urp => urp.ContractValue, cancellationToken);

        var unsignedPlayersQuery = _dbContext.Players
            .Where(p => p.TeamId == request.TeamId)
            .WhereIsUnsigned();
        var unsignedPlayers = await unsignedPlayersQuery
            .ProjectTo<PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        teamDetail.UnsignedPlayers.AddRange(unsignedPlayers);

        var unsignedPlayersContractValue = await unsignedPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

        teamDetail.CapSpace = CapSpaceUtilities.CalculateCurrentCapSpace(DateOnly.FromDateTime(DateTime.Today), rosteredPlayersContractValue, unrosteredPlayersContractValue, unsignedPlayersContractValue).ToString("C0");

        return teamDetail;
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Player, PlayerItem>()
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")));

        CreateMap<Team, TeamDetailResult>();
    }
}
