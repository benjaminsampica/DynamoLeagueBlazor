using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Players;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Players;

[ApiController]
[Route("api/players")]
public class ListController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<PlayerListResult> GetAsync(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListQuery(), cancellationToken);
    }
}

public record ListQuery : IRequest<PlayerListResult> { }

public class ListHandler : IRequestHandler<ListQuery, PlayerListResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PlayerListResult> Handle(ListQuery request, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Include(p => p.Team)
            .ProjectTo<PlayerListResult.PlayerItem>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PlayerListResult
        {
            Players = players
        };
    }
}

public class ListMappingProfile : Profile
{
    public ListMappingProfile()
    {
        CreateMap<Player, PlayerListResult.PlayerItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.ContractValue, mo => mo.MapFrom(s => s.ContractValue.ToString("C0")))
            .ForMember(d => d.YearContractExpires, mo => mo.MapFrom(s => s.YearContractExpires.ToString()));
    }
}
