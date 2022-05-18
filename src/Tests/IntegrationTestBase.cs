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
using Respawn.Graph;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DynamoLeagueBlazor.Tests;

[Collection(nameof(Server))]
public class IntegrationTestBase : IAsyncLifetime
{
    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync() => await ResetStateAsync();
}


[CollectionDefinition(nameof(Server))]
public class IntegrationTesting : ICollectionFixture<IntegrationTesting>, IAsyncLifetime
{
    private static Checkpoint _checkpoint = null!;
    private static IConfiguration _configuration = null!;
    internal static WebApplicationFactory<Program> _setupApplication = null!;

    public async Task InitializeAsync()
    {
        _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        };

        _setupApplication = new TestWebApplicationFactory(_configuration);

        using var scope = _setupApplication.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _setupApplication.DisposeAsync();
    }

    public static async Task ResetStateAsync()
    {
        await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }

    internal static WebApplicationFactory<Program> CreateUserAuthenticatedApplication()
        => CreateApplication()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(UserAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(UserAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> CreateAdminAuthenticatedApplication()
        => CreateApplication()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(AdminAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>(AdminAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> CreateUnauthenticatedApplication()
        => CreateApplication();

    private static WebApplicationFactory<Program> CreateApplication()
    {
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

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthorization(options =>
            {
                options.AddApplicationAuthorizationPolicies();
            });
        });
    }
}

internal static class IntegrationTestExtensions
{
    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this WebApplicationFactory<Program> application)
        where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync();
    }

    public static async Task AddAsync<TEntity>(this WebApplicationFactory<Program> application, TEntity entity)
        where TEntity : class
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task UpdateAsync<TEntity>(this WebApplicationFactory<Program> application, TEntity entity)
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
    private readonly ApplicationDbContext _applicationDbContext;
    public static int TeamId = 1;

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

        return AuthenticationHandlerUtilities.GetSuccessfulAuthenticateResult(AuthenticationName, TeamId, AuthenticationName);
    }
}

internal class AdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationName = RoleName.Admin;
    private readonly ApplicationDbContext _applicationDbContext;
    public static int TeamId = 1;

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

        return AuthenticationHandlerUtilities.GetSuccessfulAuthenticateResult(AuthenticationName, TeamId, AuthenticationName);
    }
}

internal static class AuthenticationHandlerUtilities
{
    public static AuthenticateResult GetSuccessfulAuthenticateResult(string role, int teamId, string authenticationName)
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, RandomString),
            new Claim(ClaimTypes.Role, role),
            new Claim(nameof(ApplicationUser.TeamId), teamId.ToString()),
            new Claim(nameof(ApplicationUser.Approved), bool.TrueString)
        };
        var identity = new ClaimsIdentity(claims, authenticationName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, authenticationName);

        var result = AuthenticateResult.Success(ticket);

        return result;
    }
}
