using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Route("api/freeagents/addbid")]
[ApiController]
public class AddBidController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBidAmountValidator _bidAmountValidator;

    public AddBidController(IMediator mediator, IMapper mapper, IBidAmountValidator bidAmountValidator)
    {
        _mediator = mediator;
        _mapper = mapper;
        _bidAmountValidator = bidAmountValidator;
    }

    [HttpGet]
    public async Task<bool> GetAsync([FromQuery] int playerId, int amount, CancellationToken cancellationToken)
    {
        var isValidBid = await _bidAmountValidator.IsHighestBidAsync(new AddBidRequest { Amount = amount, PlayerId = playerId }, cancellationToken);

        return isValidBid;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddBidRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<AddBidCommand>(request);

        return await _mediator.Send(query, cancellationToken);
    }
}

public record AddBidCommand(int PlayerId, int Amount) : IRequest<int> { }

public class AddBidHandler : IRequestHandler<AddBidCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddBidHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> Handle(AddBidCommand request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Players
            .AsTracking()
            .SingleAsync(p => p.Id == request.PlayerId, cancellationToken);

        var currentUserTeamId = _httpContextAccessor.HttpContext!.User.GetTeamId();
        var bid = player!.AddBid(request.Amount, currentUserTeamId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return bid.Id;
    }
}

public class AddBidMappingProfile : Profile
{
    public AddBidMappingProfile()
    {
        CreateMap<AddBidRequest, AddBidCommand>();
    }
}

public class BidAmountValidator : IBidAmountValidator
{
    private readonly ApplicationDbContext _dbContext;

    public BidAmountValidator(ApplicationDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<bool> IsHighestBidAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var isHighestBid = await _dbContext.Players
            .Where(p => p.Id == request.PlayerId
                && p.Bids.All(b => request.Amount > b.Amount))
            .AnyAsync(cancellationToken);

        return isHighestBid;
    }
}
