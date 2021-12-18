using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

[Authorize]
[ApiController]
[Route("freeagents")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<FreeAgentListResult> GetAsync()
    {
        return await _mediator.Send(new ListQuery());
    }
}

public record ListQuery : IRequest<FreeAgentListResult> { }

public class ListHandler : IRequestHandler<ListQuery, FreeAgentListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<FreeAgentListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var freeAgents = await _dbContext.Players
            .Include(p => p.Team)
            .Include(p => p.Bids)
            .WhereIsFreeAgent()
            .ProjectTo<FreeAgentListResult.FreeAgentItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new FreeAgentListResult
        {
            FreeAgents = freeAgents
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Player, FreeAgentListResult.FreeAgentItem>()
            .ForMember(d => d.PlayerTeam, mo => mo.MapFrom(s => s.Team != null ? s.Team.TeamName : string.Empty))
            .ForMember(d => d.BiddingEnds, mo => mo.MapFrom(s => s.EndOfFreeAgency!.Value.ToShortDateString()))
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.HeadShot))
            .ForMember(d => d.HighestBid, mo => mo.MapFrom(s =>
                s.Bids.Any()
                ? s.Bids.OrderByDescending(b => b.Amount).First().Amount.ToString("C0")
                : string.Empty)
            );
    }
}
