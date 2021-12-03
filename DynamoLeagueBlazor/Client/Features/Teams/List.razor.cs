using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Teams;

[Authorize]
public partial class List
{
    //private WeatherForecast[]? forecasts;

    //protected override async Task OnInitializedAsync()
    //{
    //    try
    //    {
    //        forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
    //    }
    //    catch (AccessTokenNotAvailableException exception)
    //    {
    //        exception.Redirect();
    //    }
    //}
}
