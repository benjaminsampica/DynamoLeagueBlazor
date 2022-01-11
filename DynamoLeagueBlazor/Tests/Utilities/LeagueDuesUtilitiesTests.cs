using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities;

internal class LeagueDuesUtilitiesTests
{
    [TestCase("1/1/2020", ExpectedResult = 105, Description = "Year 2020 dues should be 105")]
    [TestCase("1/1/2021", ExpectedResult = 105, Description = "Year 2021 dues should be 105")]
    [TestCase("1/1/2022", ExpectedResult = 110, Description = "Year 2022 dues should be 110")]
    public int GivenDifferentDates_ThenReturnsExpectedResult(string dateString)
    {
        var date = DateOnly.Parse(dateString);

        return LeagueDuesUtilities.GetCurrentLeagueDues(date);
    }
}
