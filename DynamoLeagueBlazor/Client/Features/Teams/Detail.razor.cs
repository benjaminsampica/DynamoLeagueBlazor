using Microsoft.AspNetCore.Components;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public partial class Detail
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    [Parameter] public int TeamId { get; set; }

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }
}
