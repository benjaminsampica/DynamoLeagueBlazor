using AutoMapper;
using AutoMapper.QueryableExtensions;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.FreeAgents;

[ApiController]
[Route("api/freeagents/")]
public class DetailController : ControllerBase
{
    private readonly IMediator _mediator;

    public DetailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{PlayerId}")]
    public async Task<FreeAgentDetailResult> GetAsync([FromRoute] DetailQuery query, CancellationToken cancellationToken)
    {
        return await _mediator.Send(query, cancellationToken);
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
            .Include(p => p.Bids)
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
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.Bids, mo => mo.MapFrom(s => s.Bids.OrderByDescending(b => b.CreatedOn)));
        CreateMap<Bid, FreeAgentDetailResult.BidItem>()
            .ForMember(d => d.Team, mo => mo.MapFrom(s => s.Team.Name))
            .ForMember(d => d.Amount, mo => mo.MapFrom(s => s.Amount.ToString("C0")))
            .ForMember(d => d.CreatedOn, mo => mo.MapFrom(s => s.CreatedOn.ToShortDateString()));
    }
}