using DynamoLeagueBlazor.Client.Shared.Components;
using MudBlazor;

namespace DynamoLeagueBlazor.Tests.Shared.Components;

public class LoadingButtonTests : UITestBase
{
    [Fact]
    public void WhenIsFirstRendered_ThenShowsSaveAndIsEnabled()
    {
        var component = RenderComponent<LoadingButton>();

        component.Markup.Should().Contain("Save");

        var button = component.FindComponent<MudButton>();

        button.Instance.Disabled.Should().BeFalse();
    }

    [Fact]
    public void WhenIsLoading_ThenShowsLoadingContentAndDisablesButton()
    {
        var component = RenderComponent<LoadingButton>(parameters =>
        {
            parameters.Add(p => p.IsLoading, true);
        });

        component.Markup.Should().Contain("Please wait...");
        var button = component.FindComponent<MudButton>();

        button.Instance.Disabled.Should().BeTrue();
    }
}
