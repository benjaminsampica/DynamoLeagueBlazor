using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize]
[Route("freeagents/addbid")]
[ApiController]
public class AddBidController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddBidController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddBidRequest request)
    {
        var query = _mapper.Map<AddBidQuery>(request);

        return await _mediator.Send(query);
    }
}

public record AddBidQuery(int PlayerId, int Amount) : IRequest<int> { }

public class AddBidHandler : IRequestHandler<AddBidQuery, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddBidHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> Handle(AddBidQuery request, CancellationToken cancellationToken)
    {
        var player = (await _dbContext.Players.FindAsync(new object?[] { request.PlayerId }, cancellationToken));

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
        CreateMap<AddBidRequest, AddBidQuery>();
    }
}

public class AddBidRequestValidator : AbstractValidator<AddBidRequest>
{
    public AddBidRequestValidator(ApplicationDbContext dbContext)
    {
        RuleFor(x => x.Amount).MustAsync(async (request, value, context, cancellationToken) =>
        {
            var player = await dbContext.Players
                .Where(p => p.Id == request.PlayerId
                    && p.Bids.GetHighestBidder().Amount > value)
                .SingleOrDefaultAsync(cancellationToken);

            return player != null;
        });
    }
}