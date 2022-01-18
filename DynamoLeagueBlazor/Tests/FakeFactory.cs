using AutoBogus;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;

namespace DynamoLeagueBlazor.Tests;

internal class FakeFactory
{
    public FakeFactory()
    {
        AutoFaker.Configure(builder =>
        {
            builder
              .WithRecursiveDepth(0)
              .WithTreeDepth(0);
        });
    }

    public static Team CreateFakeTeam()
    {
        var faker = new AutoFaker<Team>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Players);

        return faker.Generate();
    }

    public static Player CreateFakePlayer()
    {
        var faker = new AutoFaker<Player>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Bids)
            .Ignore(f => f.TeamId)
            .Ignore(f => f.Team)
            .Ignore(f => f.Fines);

        return faker.Generate();
    }

    public static Fine CreateFakeFine(int playerId)
    {
        var faker = new AutoFaker<Fine>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Player)
            .RuleFor(f => f.PlayerId, playerId);

        return faker.Generate();
    }

    public static Bid CreateFakeBid()
    {
        var faker = new AutoFaker<Bid>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Player)
            .Ignore(f => f.PlayerId)
            .Ignore(f => f.Team)
            .Ignore(f => f.TeamId);

        return faker.Generate();
    }

    public static ApplicationUser CreateFakeUser(int teamId)
        => new("fake@gmail.com", teamId);

    public static string RandomString => AutoFaker.Generate<string>();
}
