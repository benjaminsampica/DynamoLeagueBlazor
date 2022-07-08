using DynamoLeagueBlazor.Client.Features.FreeAgents;

namespace DynamoLeagueBlazor.Tests.Features.FreeAgents;

public class BidCountdownTests : UITestBase
{
    [Fact]
    public void DisplaysAHumanReadableTime()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddSeconds(1));
        });

        cut.WaitForState(() => cut.Markup.Contains("seconds"));
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
    public void GivenMoreThanADayAway_ThenShowsRedText()
    {
        var cut = RenderComponent<BidCountdown>(parameters =>
        {
            parameters.Add(p => p.DateTime, DateTime.Now.AddDays(1));
        });

        cut.Markup.Contains("mud-text-warning");
    }
}
