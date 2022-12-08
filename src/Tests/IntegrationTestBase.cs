using DynamoLeagueBlazor.Server.Infrastructure;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Infrastructure;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;
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

    internal static WebApplicationFactory<Program> GetUserAuthenticatedApplication(int teamIdToImpersonate = 1)
        => _application
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(RoleName.User)
                        .AddScheme<ImpersonationAuthenticationSchemeOptions, ImpersonationAuthenticationHandler>(RoleName.User,
                            options =>
                            {
                                options.TeamIdToImpersonate = teamIdToImpersonate;
                                options.RoleName = RoleName.User;
                            }
                        );

                    ReplaceCurrentUserService(services, teamIdToImpersonate);
                });
            });

    internal static WebApplicationFactory<Program> GetAdminAuthenticatedApplication(int teamIdToImpersonate = 1)
        => _application
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(RoleName.Admin)
                        .AddScheme<ImpersonationAuthenticationSchemeOptions, ImpersonationAuthenticationHandler>(RoleName.Admin,
                            options =>
                            {
                                options.TeamIdToImpersonate = teamIdToImpersonate;
                                options.RoleName = RoleName.Admin;
                            }
                        );

                    ReplaceCurrentUserService(services, teamIdToImpersonate);
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
                    var playerHeadshotService = services.Single(d => d.ServiceType == typeof(IPlayerHeadshotService));
                    services.Remove(playerHeadshotService);
                    var stubPlayerHeadshotService = new Mock<IPlayerHeadshotService>();
                    stubPlayerHeadshotService.Setup(phs => phs.FindPlayerHeadshotUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(RandomString);
                    services.AddSingleton(stubPlayerHeadshotService.Object);

                    ReplaceCurrentUserService(services, 1);
                });
            });

        return application;
    }

    private static void ReplaceCurrentUserService(IServiceCollection services, int teamIdToImpersonate)
    {
        var currentUserService = services.Single(d => d.ServiceType == typeof(ICurrentUserService));
        services.Remove(currentUserService);
        var stubCurrentUserService = new Mock<ICurrentUserService>();
        stubCurrentUserService.Setup(cus => cus.GetTeamId())
            .Returns(teamIdToImpersonate);
        services.AddSingleton(stubCurrentUserService.Object);
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

internal class ImpersonationAuthenticationHandler : AuthenticationHandler<ImpersonationAuthenticationSchemeOptions>
{
    public ImpersonationAuthenticationHandler(
        IOptionsMonitor<ImpersonationAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, RandomString),
            new Claim(ClaimTypes.Role, Options.RoleName),
            new Claim(nameof(ApplicationUser.TeamId), Options.TeamIdToImpersonate.ToString()),
            new Claim(nameof(ApplicationUser.Approved), bool.TrueString)
        };
        var identity = new ClaimsIdentity(claims, Options.RoleName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Options.RoleName);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

internal class ImpersonationAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public int TeamIdToImpersonate { get; set; }
    public string RoleName { get; set; } = null!;
}