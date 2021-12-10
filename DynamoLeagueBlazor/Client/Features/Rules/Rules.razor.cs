using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Rules;

[Authorize]
public partial class Rules
{
    private const string _title = "Rules";
}
