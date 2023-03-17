using Coravel;
using DotNet.Testcontainers.Configurations;
using Duende.IdentityServer.Services;
using DynamoLeagueBlazor.Server.Areas.Identity;
using DynamoLeagueBlazor.Server.Features.Admin.Shared;
using DynamoLeagueBlazor.Server.Features.FreeAgents;
using DynamoLeagueBlazor.Server.Features.FreeAgents.Detail;
using DynamoLeagueBlazor.Server.Features.OfferMatching;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Features.FreeAgents.Detail;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infrastructure;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();

    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Duende", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName();

    if (builder.Environment.IsDevelopment())
    {
        loggerConfiguration.MinimumLevel.Is(LogEventLevel.Verbose);
        loggerConfiguration.WriteTo.Debug();
    }
    else
    {
        loggerConfiguration.MinimumLevel.Is(LogEventLevel.Information);
        loggerConfiguration.WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Name} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
    }

    Log.Logger = loggerConfiguration.CreateLogger();

    builder.Host.UseSerilog();

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsDevelopment())
    {
        var logger = LoggerFactory.Create(logging => logging.AddSerilog(Log.Logger)).CreateLogger<Program>();
        TestcontainersSettings.Logger = logger;
        connectionString = await MsSqlContainerFactory.CreateAsync();
    }

    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(connectionString);
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.EnableSensitiveDataLogging();
    }, ServiceLifetime.Scoped);

    builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddClaimsPrincipalFactory<CurrentUserClaimsFactory>();


    builder.Services.AddIdentityServer().AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

    builder.Services.AddScoped<IProfileService, ProfileService>();
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddAuthentication()
        .AddIdentityServerJwt();

    builder.Services.AddAuthorization(options =>
    {
        options.AddApplicationAuthorizationPolicies();
    });

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
    builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
    builder.Services.AddValidatorsFromAssemblyContaining<AddFineRequestValidator>();
    builder.Services.AddFluentValidationAutoValidation();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
    });
    builder.Services.AddScoped<IBidValidator, BidValidator>();
    builder.Services.AddScoped<IPlayerHeadshotService, PlayerHeadshotService>();
    builder.Services.AddScoped<IMatchPlayerValidator, MatchPlayerValidator>();

    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.Email))
        .AddSingleton(s => s.GetRequiredService<IOptions<EmailSettings>>().Value);

    if (builder.Environment.IsProduction())
    {
        builder.Services.AddSingleton<IEmailSender, EmailSender>();
    }
    else
    {
        builder.Services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
    }

    builder.Services.AddScheduler();
    builder.Services.AddScoped<EndBiddingService>();
    builder.Services.AddScoped<ExpireOfferMatchingService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else if (app.Environment.IsProduction())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();

        app.Services.UseScheduler(scheduler =>
        {
            // All bidding closes at 10PM CST.
            var centralStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

            scheduler.Schedule<EndBiddingService>()
                .DailyAtHour(22)
                .Zoned(centralStandardTimeZone)
                .RunOnceAtStart();

            scheduler.Schedule<ExpireOfferMatchingService>()
                .Daily()
                .Zoned(centralStandardTimeZone)
                .RunOnceAtStart();
        }).OnError((ex) =>
        {
            Log.Error(ex, "An exception occured when trying to run a scheduled job.");
        });
    }

    app.UseRewriter(new RewriteOptions()
        .AddRedirectToWww()
        .AddRedirectToHttps((int)HttpStatusCode.TemporaryRedirect));

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseIdentityServer();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Use((context, next) =>
    {
        using var _ = LogContext.PushProperty(nameof(ClaimTypes.Name), context.User.FindFirst(ClaimTypes.Name)?.Value);
        return next(context);
    });

    app.UseSerilogIngestion();
    app.UseSerilogRequestLogging();

    app.MapRazorPages();
    app.MapControllers().RequireAuthorization();
    app.MapFallbackToFile("index.html");

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await applicationDbContext.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new SeedDataCommand());
        }
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the API host.");
}
finally
{
    Log.CloseAndFlush();
}

