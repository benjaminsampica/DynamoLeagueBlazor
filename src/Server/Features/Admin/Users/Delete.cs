using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Users;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DynamoLeagueBlazor.Server.Features.Admin.Users;

[Authorize(Roles = RoleName.Admin)]
[ApiController]
[Route("api/admin/users")]
public class DeleteController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete]
    public async Task DeleteAsync([FromQuery] DeleteUserRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCommand(request.UserId), cancellationToken);
    }
}

public record DeleteCommand(string UserId) : IRequest { }

public class DeleteHandler : IRequestHandler<DeleteCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        await _userManager.DeleteAsync(user);

        return Unit.Value;
    }
}
