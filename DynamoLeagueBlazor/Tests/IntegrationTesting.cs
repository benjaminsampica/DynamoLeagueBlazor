using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
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
    public async Task RunBeforeAnyTestsAsync()
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

        var application = new TestWebApplicationFactory(_configuration);

        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public static async Task ResetStateAsync()
    {
        await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }

    internal static WebApplicationFactory<Program> CreateUserAuthenticatedApplication(Action<WebApplicationFactoryClientOptions>? options = null, int userTeamId = 1)
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
    }
}

internal static class IntegrationTestExtensions
{
    internal static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this WebApplicationFactory<Program> application)
        where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync();
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

internal class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = RoleName.User;
    public static int TeamId = 1;
    private readonly ApplicationDbContext _applicationDbContext;

    public UserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ApplicationDbContext applicationDbContext)
        : base(options, logger, encoder, clock)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        TeamId = (await _applicationDbContext.Teams.FirstOrDefaultAsync())?.Id ?? TeamId;

        var claims = new[] {
            new Claim(ClaimTypes.Name, RandomString),
            new Claim(ClaimTypes.Role, RoleName.User),
            new Claim(nameof(ApplicationUser.TeamId), TeamId.ToString())
        };
        var identity = new ClaimsIdentity(claims, AuthenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationName);

        var result = AuthenticateResult.Success(ticket);

        return result;
    }
}

internal class AdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = RoleName.Admin;
    public static int TeamId = 1;
    private readonly ApplicationDbContext _applicationDbContext;

    public AdminAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ApplicationDbContext applicationDbContext)
        : base(options, logger, encoder, clock)
    {
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        TeamId = (await _applicationDbContext.Teams.FirstOrDefaultAsync())?.Id ?? TeamId;

        var claims = new[] {
            new Claim(ClaimTypes.Name, RandomString),
            new Claim(ClaimTypes.Role, RoleName.Admin),
            new Claim(nameof(ApplicationUser.TeamId), TeamId.ToString())
        };
        var identity = new ClaimsIdentity(claims, AuthenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationName);

        var result = AuthenticateResult.Success(ticket);

        return result;
    }
}