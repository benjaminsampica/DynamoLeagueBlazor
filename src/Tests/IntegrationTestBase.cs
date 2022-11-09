using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Infastructure;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
    private static WebApplicationFactory<Program> _application = null!;
    private static IServiceScope _scope = null!;
    private static string _connectionString = null!;

    public async Task InitializeAsync()
    {
        _connectionString = await MsSqlContainerFactory.CreateAsync();

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        };

        _application = CreateApplication();

        _scope = _application.Services.CreateAsyncScope();

        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _application.DisposeAsync();
    }

    public static async Task ResetStateAsync()
    {
        await _checkpoint.Reset(_connectionString);
    }

    internal static WebApplicationFactory<Program> GetUserAuthenticatedApplication()
        => _application
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(UserAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(UserAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> GetAdminAuthenticatedApplication()
        => _application
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(AdminAuthenticationHandler.AuthenticationName)
                        .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>(AdminAuthenticationHandler.AuthenticationName, options => { });
                });
            });

    internal static WebApplicationFactory<Program> GetUnauthenticatedApplication()
        => _application;

    private static WebApplicationFactory<Program> CreateApplication()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);

                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthorization(options =>
                    {
                        options.AddApplicationAuthorizationPolicies();
                    });

                    // Stub out calls to Player Profiler.
                    var descriptor = services.Single(d => d.ServiceType == typeof(IPlayerHeadshotService));
                    services.Remove(descriptor);
                    var stubPlayerHeadshotService = new Mock<IPlayerHeadshotService>();
                    stubPlayerHeadshotService.Setup(phs => phs.FindPlayerHeadshotUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(RandomString);
                    services.AddSingleton(stubPlayerHeadshotService.Object);
                });
            });

        return application;
    }

    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>()
        where TEntity : class
    {
        var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync();
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task UpdateAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Update(entity);

        await context.SaveChangesAsync();
    }

    public static T GetRequiredService<T>() where T : class =>
        _application.Services.GetRequiredService<T>();
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
