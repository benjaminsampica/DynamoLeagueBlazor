using DynamoLeagueBlazor.Shared.Features.Admin.Users;
using Microsoft.AspNetCore.WebUtilities;

namespace DynamoLeagueBlazor.Client.Features.Admin.Users;

public sealed partial class Delete : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public string UserId { get; set; } = null!;

    private DeleteUserRequest _form = null!;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        _form = new DeleteUserRequest { UserId = UserId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var uri = DeleteUserRouteFactory.Create(_form);
        var response = await HttpClient.DeleteAsync(uri, _cts.Token);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully deleted the user.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public static class DeleteUserRouteFactory
{
    public static string Create(DeleteUserRequest request)
    {
        var uri = QueryHelpers.AddQueryString("api/admin/users", nameof(DeleteUserRequest.UserId), request.UserId);

        return uri;
    }
}
