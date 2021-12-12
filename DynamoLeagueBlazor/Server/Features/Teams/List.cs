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
            .ProjectTo<TeamItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        foreach (var team in teams)
        {
            var rosteredPlayersQuery = _dbContext.Players
                .Where(p => p.TeamId == team.Id)
                .WhereIsRostered();
            var rosteredPlayerCount = await rosteredPlayersQuery.CountAsync(cancellationToken);
            team.RosteredPlayerCount = rosteredPlayerCount.ToString();

            var rosteredPlayersContractValue = await rosteredPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

            var unrosteredPlayersQuery = _dbContext.Players
                .Where(p => p.TeamId == team.Id)
                .WhereIsUnrostered();
            var unrosteredPlayerCount = await unrosteredPlayersQuery.CountAsync(cancellationToken);
            team.UnrosteredPlayerCount = unrosteredPlayerCount.ToString();

            var unrosteredPlayersContractValue = await unrosteredPlayersQuery.SumAsync(urp => urp.ContractValue, cancellationToken);

            var unsignedPlayersQuery = _dbContext.Players
                .Where(p => p.TeamId == team.Id)
                .WhereIsUnsigned();
            var unsignedPlayerCount = await unsignedPlayersQuery.CountAsync(cancellationToken);
            team.UnsignedPlayerCount = unsignedPlayerCount.ToString();

            var unsignedPlayersContractValue = await unsignedPlayersQuery.SumAsync(rp => rp.ContractValue, cancellationToken);

            team.CapSpace = CapSpaceUtilities.CalculateCurrentCapSpace(rosteredPlayersContractValue, unrosteredPlayersContractValue, unsignedPlayersContractValue).ToString("C0");
        }

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
        CreateMap<Team, TeamItem>();
    }
}
