using AutoBogus;
using DynamoLeagueBlazor.Server.Infrastructure.Identity;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Enums;

namespace DynamoLeagueBlazor.Tests;

public class FakeFactory
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
            .Ignore(f => f.Players)
            .Ignore(f => f.Fines);

        return faker.Generate();
    }

    public static Player CreateFakePlayer(int? teamId = null)
    {
        var faker = new AutoFaker<Player>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Bids)
            .Ignore(f => f.Team)
            .Ignore(f => f.Fines)
            .RuleFor(f => f.TeamId, teamId ?? null);

        return faker.Generate();
    }

    public static Fine CreateFakeFine(int playerId, int teamId)
    {
        var faker = new AutoFaker<Fine>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Player)
            .Ignore(f => f.Team)
            .RuleFor(f => f.PlayerId, playerId)
            .RuleFor(f => f.TeamId, teamId);

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

    public static FakePosition CreateFakePosition() => new();

    public static string RandomString => AutoFaker.Generate<string>();
}

public class FakePosition : Position
{
    public FakePosition() : base(nameof(FakePosition), 0) { }

    public override int[] PerYearContractPriceTable() => new[] { 1, 2 };
}