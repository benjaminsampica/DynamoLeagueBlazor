namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public interface ICurrentUserService
{
    int GetTeamId();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public int GetTeamId() => _currentUserService.GetTeamId();
}
