using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

[Authorize]
[ApiController]
[Route("freeagents/")]
public class DetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public DetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{PlayerId}")]
    public async Task<FreeAgentDetailResult> GetAsync([FromRoute] DetailQuery query)
    {
        return await _mediator.Send(query);
    }
}

public record DetailQuery(int PlayerId) : IRequest<FreeAgentDetailResult> { }

public class DetailHandler : IRequestHandler<DetailQuery, FreeAgentDetailResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public DetailHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<FreeAgentDetailResult> Handle(DetailQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Players
            .Include(p => p.Bids.OrderByDescending(b => b.DateTime))
                .ThenInclude(b => b.Team)
            .Where(p => p.Id == request.PlayerId)
            .ProjectTo<FreeAgentDetailResult>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        return result;
    }
}

public class DetailValidator : AbstractValidator<DetailQuery>
{
    public DetailValidator(ApplicationDbContext dbContext)
    {
        RuleFor(p => p.PlayerId)
            .Must(value => dbContext.Players.Any(p => p.Id == value));
    }
}

public class DetailMappingProfile : Profile
{
    public DetailMappingProfile()
    {
        CreateMap<Player, FreeAgentDetailResult>()
            .ForMember(d => d.EndOfFreeAgency, mo => mo.MapFrom(s => s.EndOfFreeAgency!.Value.ToShortDateString()))
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.TeamName));
        CreateMap<Bid, FreeAgentDetailResult.BidItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.TeamName))
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Amount.ToString("C0")))
            .ForMember(d => d.Date, mo => mo.MapFrom(s => s.DateTime.ToShortDateString()));
    }
}