using DynamoLeagueBlazor.Client.Features.FreeAgents;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class BidCountdownTests : UITestBase
{
    [Fact]
    public void CountsDownEverySecondUntilZero()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddSeconds(4));
        });

        cut.WaitForState(() => cut.Markup.Contains("2 seconds"));
        cut.WaitForState(() => cut.Markup.Contains("1 second"));
        cut.WaitForState(() => cut.Markup.Contains("0 seconds"));

        cut.Render();
        cut.Markup.Should().Contain("0 seconds");
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
