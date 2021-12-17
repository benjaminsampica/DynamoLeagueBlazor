using DynamoLeagueBlazor.Server.Areas.Identity;
using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
    private static IConfiguration _configuration = null!;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        };

        // Create the database.
        var _ = new TestWebApplicationFactory(_configuration).CreateClient();
    }

    public static async Task ResetStateAsync()
    {
        await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }

    internal static WebApplicationFactory<Program> CreateUserAuthenticatedApplication(Action<WebApplicationFactoryClientOptions>? options = null)
        => CreateApplication(options)
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(UserAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(UserAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> CreateAdminAuthenticatedApplication(Action<WebApplicationFactoryClientOptions>? options = null)
        => CreateApplication(options)
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(AdminAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>(AdminAuthenticationHandler.AuthenticationName, options => { });
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

        var application = new TestWebApplicationFactory(_configuration);
        _configuration = application.Services.GetRequiredService<IConfiguration>();

        return application;
    }
}

internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public TestWebApplicationFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((builderContext, config) =>
        {
            config.AddConfiguration(_configuration);
        });

        builder.ConfigureServices(async services =>
        {
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
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

    internal static async Task UpdateAsync<TEntity>(this WebApplicationFactory<Program> application, TEntity entity)
        where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Update(entity);

        await context.SaveChangesAsync();
    }
}

internal record TestHttpResponse<TContent>(HttpResponseMessage Message, TContent? Content) where TContent : class;

internal class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = "User";

    public UserAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user"), new Claim(ClaimTypes.Role, ApplicationRole.User) };
        var identity = new ClaimsIdentity(claims, AuthenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationName);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

internal class AdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = "Admin";

    public AdminAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user"), new Claim(ClaimTypes.Role, ApplicationRole.Admin) };
        var identity = new ClaimsIdentity(claims, AuthenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationName);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}