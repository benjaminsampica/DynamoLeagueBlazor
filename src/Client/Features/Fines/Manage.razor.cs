using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Client.Features.Fines;

public partial class Manage
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }
    [CascadingParameter] public required MudDialogInstance MudDialogInstance { get; set; }
    [Parameter, EditorRequired] public required int FineId { get; set; }

    private ManageFineRequest _form = null!;
    private bool _processingForm;

    protected override void OnInitialized()
    {
        _form = new ManageFineRequest { FineId = FineId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(ManageFineRouteFactory.Uri, _form);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully updated fine.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    private void IsApproved(bool approved)
    {
        _form.Approved = approved;
    }
}
