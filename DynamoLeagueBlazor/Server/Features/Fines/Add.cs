using AutoMapper;
using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamoLeagueBlazor.Server.Features.Fines;

[Authorize]
[Route("fines/add")]
[ApiController]
public class AddController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AddController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<int> PostAsync([FromBody] AddFineRequest request)
    {
        var query = _mapper.Map<AddQuery>(request);

        return await _mediator.Send(query);
    }
}

public record AddQuery(int PlayerId, string FineReason) : IRequest<int> { }

public class AddHandler : IRequestHandler<AddQuery, int>
{
    private readonly ApplicationDbContext _dbContext;

    public AddHandler(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(AddQuery request, CancellationToken cancellationToken)
    {
        // TODO: Calculate amount
        var amount = 1;
        var fine = new Fine(amount, request.FineReason, request.PlayerId);

        _dbContext.Fines.Add(fine);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return fine.Id;
    }
}

public class AddMappingProfile : Profile
{
    public AddMappingProfile()
    {
        CreateMap<AddFineRequest, AddQuery>();
    }
}