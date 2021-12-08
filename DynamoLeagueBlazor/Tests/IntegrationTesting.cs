using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Respawn;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DynamoLeagueBlazor.Tests;

[SetUpFixture]
public class IntegrationTesting
{
    private static Checkpoint _checkpoint = null!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        };
    }

    public static async Task ResetStateAsync()
    {
        // TODO: Setup localdb when its working on W11
        //await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }

    internal static WebApplicationFactory<Program> CreateAuthenticatedApplication(Action<WebApplicationFactoryClientOptions>? options = null)
        => CreateApplication(options)
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(TestAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> CreateUnauthenticatedApplication(Action<WebApplicationFactoryClientOptions>? options = null)
        => CreateApplication(options);

    private static WebApplicationFactory<Program> CreateApplication(Action<WebApplicationFactoryClientOptions>? options = null)
    {
        var clientOptions = new WebApplicationFactoryClientOptions();
        if (options is not null)
        {
            options.Invoke(clientOptions);
        }

        var application = new TestWebApplicationFactory();

        return application;
    }
}

internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((builderContext, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices(async services =>
        {
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.EnsureCreatedAsync();
        });
    }
}

internal static class IntegrationTestExtensions
{
    internal static async Task<TEntity?> FindAsync<TEntity>(this WebApplicationFactory<Program> application, int id)
    where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(id);
    }

    internal static async Task AddAsync<TEntity>(this WebApplicationFactory<Program> application, TEntity entity)
        where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }
}

internal record TestHttpResponse<TContent>(HttpResponseMessage Message, TContent? Content) where TContent : class;

internal class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = "Test";

    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, AuthenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationName);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}