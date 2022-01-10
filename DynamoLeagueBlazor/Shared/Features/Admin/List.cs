namespace DynamoLeagueBlazor.Shared.Features.Admin;

public class UserListResult
{
    public IEnumerable<UserItem> Users { get; init; } = Array.Empty<UserItem>();

    public class UserItem
    {
        public string Id { get; set; }
        public string Team { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
