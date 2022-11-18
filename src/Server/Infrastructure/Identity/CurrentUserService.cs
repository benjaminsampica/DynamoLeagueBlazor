namespace DynamoLeagueBlazor.Server.Infrastructure.Identity;

public interface ICurrentUserService
{
    int GetTeamId();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetTeamId() => _httpContextAccessor.HttpContext.User.GetTeamId();
}
