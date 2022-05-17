using DynamoLeagueBlazor.Server.Models;

namespace DynamoLeagueBlazor.Tests.Models;

public class PlayerTests
{
    [Theory]
    [InlineData(-1), InlineData(-2), InlineData(-3)]
    public void WhereIsOfferMatching_GivenAPlayer_WhenTheirFreeAgencyIsThreeOrLessDaysFromToday_ThenShouldContainThePlayer(int daysAgo)
    {
        var player = CreateFakePlayer();
        player.SetToRostered(DateTime.Today.AddYears(-1).Year, int.MaxValue);
        player.SetToFreeAgent(DateTime.Today.AddDays(daysAgo));

        var sut = new List<Player> { player }.AsQueryable();

        sut.WhereIsOfferMatching().Should().Contain(player);
    }

    [Fact]
    public void WhereIsOfferMatching_GivenAPlayer_WhenTheirFreeAgencyIsFourOrMoreDaysFromToday_ThenShouldNotContainThePlayer()
    {
        var player = CreateFakePlayer();
        player.SetToRostered(DateTime.Today.AddYears(-1).Year, int.MaxValue);
        player.SetToFreeAgent(DateTime.Today.AddDays(-4));

        var sut = new List<Player> { player }.AsQueryable();

        sut.WhereIsOfferMatching().Should().NotContain(player);
    }
}
