using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Dashboard;

[Authorize]
public partial class Dashboard
{
    private const string _title = "Dashboard";
}
