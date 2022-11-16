using DynamoLeagueBlazor.Client.Features.FreeAgents.Detail;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents.Detail;

public class BidCountdownTests : UITestBase
{
    [Fact]
    public void CountsDownEverySecondUntilZero()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddSeconds(2));
        });

        cut.WaitForState(() => cut.Markup.Contains("0 seconds"), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GivenLessThanADayAway_ThenShowsRedText()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddSeconds(1));
        });

        cut.Markup.Contains("mud-text-error");
    }

    [Fact]
    public void GivenMoreThanADayAway_ThenShowsYellowText()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddDays(1));
        });

        cut.Markup.Contains("mud-text-warning");
    }
}
