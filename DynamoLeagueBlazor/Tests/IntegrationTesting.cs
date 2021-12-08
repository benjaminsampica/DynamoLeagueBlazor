using DynamoLeagueBlazor.Server.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Respawn;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DynamoLeagueBlazor.Tests;

[SetUpFixture]
public class IntegrationTesting
{
    private static IConfigurationRoot _configuration = null!;
    private static IServiceProvider _serviceProvider = null!;
    private static Checkpoint _checkpoint = null!;

    internal static WebApplicationFactory<Program> Application { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((builderContext, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true, true)
                        .AddEnvironmentVariables();

                    _configuration = builder.Build();
                });
            });

        _serviceProvider = application.Services;

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        };

        Application = application;

        await EnsureDatabaseAsync();
    }

    public static async Task ResetStateAsync()
    {
        // TODO: Setup localdb when its working on W11
        //await _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }

    public static async Task<TEntity?> FindAsync<TEntity>(int id)
        where TEntity : class
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(id);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static HttpClient CreateAuthenticatedClient(Action<WebApplicationFactoryClientOptions>? options)
    {
        var clientOptions = new WebApplicationFactoryClientOptions();
        if (options is not null)
        {
            options.Invoke(clientOptions);
        }

        var client = Application
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
                });
            })
            .CreateClient(clientOptions);

        return client;
    }

    public static HttpClient CreateUnauthenticatedClient(Action<WebApplicationFactoryClientOptions>? options)
    {
        var clientOptions = new WebApplicationFactoryClientOptions();
        if (options is not null)
        {
            options.Invoke(clientOptions);
        }

        var client = Application
            .CreateClient(clientOptions);

        return client;
    }

    private static async Task EnsureDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // TODO: Setup localdb when its working on W11
        //await dynamoLeagueDbContext.Database.MigrateAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

internal static class HttpClientExtensions
{
    public static async Task<TestHttpResponseMessage<TResponse>> GetAsync<TResponse>(this HttpClient httpClient, string? requestUri)
    {
        var response = await httpClient.GetAsync(requestUri);
        var contentStream = await response.Content.ReadAsStreamAsync();
        var responseContent = await JsonSerializer.DeserializeAsync<TResponse>(contentStream);

        var testResponse = (TestHttpResponseMessage<TResponse>)response;
        testResponse.Content = responseContent;

        return testResponse;
    }
}

internal class TestHttpResponseMessage<TResponse> : HttpResponseMessage
{
    public new TResponse? Content { get; set; }
}

internal class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}