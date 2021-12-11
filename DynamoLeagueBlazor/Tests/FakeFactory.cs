using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Fines;

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
            .Ignore(f => f.Team)
            .Ignore(f => f.Fines);

        return faker.Generate();
    }

    public static Fine CreateFakeFine()
    {
        var faker = new AutoFaker<Fine>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Player);

        return faker.Generate();
    }

    public static AddFineRequest CreateFakeValidAddFineRequest()
    {
        var faker = new AutoFaker<AddFineRequest>()
            .RuleFor(f => f.PlayerId, 1);

        return faker.Generate();
    }
}
