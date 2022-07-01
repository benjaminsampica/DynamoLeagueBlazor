namespace DynamoLeagueBlazor.Shared.Features.Admin.Users;

public class UserListResult
{
    public IEnumerable<UserItem> Users { get; init; } = Array.Empty<UserItem>();

    public class UserItem
    {
        public string Id { get; set; } = null!;
        public string Team { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
    }
}
