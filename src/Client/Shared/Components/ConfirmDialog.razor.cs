using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DynamoLeagueBlazor.Client.Shared.Components;

public partial class ConfirmDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
}

public interface IConfirmDialogService
{
    Task<bool> IsCancelledAsync();
}

public class ConfirmDialogService : IConfirmDialogService
{
    private readonly IDialogService _dialogService;

    public ConfirmDialogService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task<bool> IsCancelledAsync()
    {
        var dialog = _dialogService.Show<ConfirmDialog>();
        var result = await dialog.Result;

        return result.Cancelled;
    }
}