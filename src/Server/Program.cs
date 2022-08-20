using Coravel;
using Duende.IdentityServer.Services;
using DynamoLeagueBlazor.Server.Areas.Identity;
using DynamoLeagueBlazor.Server.Features.Admin.Shared;
using DynamoLeagueBlazor.Server.Features.Fines;
using DynamoLeagueBlazor.Server.Features.FreeAgents;
using DynamoLeagueBlazor.Server.Features.OfferMatching;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Duende", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Name} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    Log.Logger = loggerConfiguration.CreateLogger();

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });
    builder.Services.AddDbContextFactory<ApplicationDbContext>(lifetime: ServiceLifetime.Scoped);

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

    // Needed for .NET 6 Identity/ASP.NET Core bug - https://github.com/dotnet/core/blob/main/release-notes/6.0/known-issues.md#spa-template-issues-with-individual-authentication-when-running-in-production
    // TODO: Revisit in .NET 7
    var issuerUri = builder.Configuration.GetValue<string>("IdentityServer:IssuerUri");
    builder.Services.AddIdentityServer(options =>
    {
        if (!string.IsNullOrEmpty(issuerUri))
        {
            options.IssuerUri = issuerUri;
        }
    }).AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

    builder.Services.AddCors(options =>
    {
        if (!string.IsNullOrEmpty(issuerUri))
        {
            options.AddDefaultPolicy(builder => builder.WithOrigins(issuerUri));
        }
    });

    builder.Services.AddTransient<IProfileService, ProfileService>();
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

    builder.Services.AddAuthentication()
        .AddIdentityServerJwt();

    builder.Services.AddAuthorization(options =>
    {
        options.AddApplicationAuthorizationPolicies();
    });

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
    builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
    builder.Services.AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AddFineRequestValidator>();
    });
    builder.Services.AddTransient<IBidValidator, BidValidator>();
    builder.Services.AddTransient<IPlayerHeadshotService, PlayerHeadshotService>();
    builder.Services.AddTransient<IMatchPlayerValidator, MatchPlayerValidator>();

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
            var centralStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

            // Hosting provider is in pacific time.
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
            Log.Logger.Error(ex, "An exception occured when trying to run a scheduled job.");
        });
    }

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors();

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

