using DynamoLeagueBlazor.Client.Shared.Components;

namespace DynamoLeagueBlazor.Tests.Shared.Components;

public class AvatarTests : UITestBase
{
    [Fact]
    public void GivenAName_ThenShowsTheInitials()
    {
        var component = RenderComponent<Avatar>(parameters =>
        {
            parameters.Add(p => p.Name, "Test Guy");
        });

        component.Markup.Should().Contain("TG");
    }

    [Fact]
    public void GivenAnImageUrl_ThenIsAnAvatar()
    {
        var component = RenderComponent<Avatar>(parameters =>
        {
            parameters.Add(p => p.Image, RandomString);
        });

        component.Markup.Should().Contain("img");
    }
}
