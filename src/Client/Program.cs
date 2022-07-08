using DynamoLeagueBlazor.Client;
using DynamoLeagueBlazor.Client.Areas.Identity;
using DynamoLeagueBlazor.Client.Features.Admin;
using DynamoLeagueBlazor.Client.Features.FreeAgents;
using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Serilog.Core;

try
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddHttpClient("DynamoLeagueBlazor.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
        .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

    // Supply HttpClient instances that include access tokens when making requests to the server project
    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DynamoLeagueBlazor.ServerAPI"));

    builder.Services.AddApiAuthorization()
        .AddAccountClaimsPrincipalFactory<ApplicationUserFactory>();

    builder.Services.AddAuthorizationCore(options =>
    {
        options.AddApplicationAuthorizationPolicies();
    });

    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
        config.SnackbarConfiguration.PreventDuplicates = false;
    });

    builder.Services.AddTransient<IBidValidator, BidValidator>();
    builder.Services.AddTransient<IPlayerHeadshotService, PlayerHeadshotService>();
    builder.Services.AddScoped<IConfirmDialogService, ConfirmDialogService>();

    var host = builder.Build();

    var levelSwitch = new LoggingLevelSwitch();
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.ControlledBy(levelSwitch)
        .WriteTo.BrowserHttp($"{builder.HostEnvironment.BaseAddress}ingest",
            controlLevelSwitch: levelSwitch,
            messageHandler: host.Services.GetRequiredService<BaseAddressAuthorizationMessageHandler>()
        ).WriteTo.BrowserConsole()
        .CreateLogger();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host.");
}
finally
{
    Log.CloseAndFlush();
}

