using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListHandler(
        ApplicationDbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FreeAgentListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var currentUserTeamId = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(nameof(ApplicationUser.TeamId))!.Value);

        var freeAgents = await _dbContext.Players
            .Include(p => p.Team)
            .Include(p => p.Bids)
            .WhereIsFreeAgent()
            .ProjectTo<FreeAgentListResult.FreeAgentItem>(_mapper.ConfigurationProvider, new { currentUserTeamId })
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
        int currentUserTeamId = 0;

        CreateMap<Player, FreeAgentListResult.FreeAgentItem>()
            .ForMember(d => d.PlayerId, mo => mo.MapFrom(s => s.Id))
            .ForMember(d => d.PlayerTeam, mo => mo.MapFrom(s => s.Team != null ? s.Team.TeamName : string.Empty))
            .ForMember(d => d.PlayerName, mo => mo.MapFrom(s => s.Name))
            .ForMember(d => d.CurrentUserIsHighestBidder, mo => mo.MapFrom(s =>
                s.Bids.Any() &&
                s.Bids.GetHighestBidder().TeamId == currentUserTeamId)
            )
            .ForMember(d => d.PlayerPosition, mo => mo.MapFrom(s => s.Position))
            .ForMember(d => d.BiddingEnds, mo => mo.MapFrom(s => s.EndOfFreeAgency!.Value.ToShortDateString()))
            .ForMember(d => d.PlayerHeadShotUrl, mo => mo.MapFrom(s => s.HeadShot))
            .ForMember(d => d.HighestBid, mo => mo.MapFrom(s =>
                s.Bids.Any()
                ? s.Bids.GetHighestBidder().Amount.ToString("C0")
                : string.Empty)
            );
    }
}
