using AutoBogus;
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
        var fakeTeam = new AutoFaker<Team>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Players);

        return fakeTeam.Generate();
    }

    public static Player CreateFakePlayer()
    {
        var fakeTeam = new AutoFaker<Player>()
            .Ignore(f => f.Id)
            .Ignore(f => f.Bids)
            .Ignore(f => f.Team);

        return fakeTeam.Generate();
    }
}
