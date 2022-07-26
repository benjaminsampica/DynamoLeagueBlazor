using DynamoLeagueBlazor.Server.Infrastructure.Identity;
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

    public static Player CreateFakePlayer()
    {
        var faker = new AutoFaker<Player>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Bids)
            .Ignore(f => f.Team)
            .Ignore(f => f.Fines)
            .Ignore(f => f.State)
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