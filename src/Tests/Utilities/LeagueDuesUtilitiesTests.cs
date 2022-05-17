using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities;

public class LeagueDuesUtilitiesTests
{
    [Theory]
    [InlineData("1/1/2020", 105)]
    [InlineData("1/1/2021", 105)]
    [InlineData("1/1/2022", 110)]
    public void GivenDifferentDates_ThenReturnsExpectedResult(string dateString, int expectedCurrentLeagueDues)
    {
        var date = DateOnly.Parse(dateString);

        LeagueDuesUtilities.GetCurrentLeagueDues(date).Should().Be(expectedCurrentLeagueDues);
    }
}
