using DynamoLeagueBlazor.Client.Shared;

namespace DynamoLeagueBlazor.Tests.Shared;

public class UpdateAvailableDetectorTests : UITestBase
{
    [Fact]
    public void WhenNoUpdateIsDetected_ThenShowsNothing()
    {
        TestContext!.JSInterop.Setup<object>("registerForUpdateAvailableNotification", _ => true);
        var component = RenderComponent<UpdateAvailableDetector>();

        component.Markup.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenAnUpdateIsDetected_ThenShowsAnAlert()
    {
        TestContext!.JSInterop.Setup<object>("registerForUpdateAvailableNotification", _ => true);
        var component = RenderComponent<UpdateAvailableDetector>();

        await component.Instance.OnUpdateAvailableAsync();

        component.HasComponent<MudAlert>().Should().BeTrue();
    }
}
