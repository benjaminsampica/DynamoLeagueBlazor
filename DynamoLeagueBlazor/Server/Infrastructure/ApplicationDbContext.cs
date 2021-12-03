using Duende.IdentityServer.EntityFramework.Options;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Shared.Entities;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DynamoLeagueBlazor.Server.Infrastructure;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    public DbSet<Bid> Bids { get; set; } = null!;
    public DbSet<Fine> Fines { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;

}
