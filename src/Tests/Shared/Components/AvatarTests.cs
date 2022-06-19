using DynamoLeagueBlazor.Client.Shared.Components;

namespace DynamoLeagueBlazor.Tests.Shared.Components;

public class AvatarTests : UITestBase
{
    [Fact]
    public void GivenNoImageUrl_ThenIsAnIcon()
    {
        var component = RenderComponent<Avatar>();

        component.Markup.Should().Contain("icon");
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
