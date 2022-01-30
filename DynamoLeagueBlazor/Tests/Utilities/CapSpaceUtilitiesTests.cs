using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities
{
    public class CapSpaceUtilitiesTests
    {
        [Theory]
        [InlineData(0, 0, 0, 1050)]
        [InlineData(500, 0, 0, 550)]
        [InlineData(0, 200, 0, 950)]
        [InlineData(0, 0, 100, 950)]
        [InlineData(600, 200, 100, 250)]
        public void GivenDifferentPlayerValuesForToday_ThenReturnsExpectedRemainingCapSpace(int rostered, int unrostered, int dropped, int expectedRemainingCapSpace)
        {
            var today = new DateOnly(2021, 01, 01);

            CapSpaceUtilities.GetRemainingCapSpace(today, rostered, unrostered, dropped).Should().Be(expectedRemainingCapSpace);
        }

        [Theory]
        [InlineData("01/01/2021", 1050)]
        [InlineData("01/01/2022", 1100)]
        [InlineData("01/01/2023", 1100)]
        [InlineData("01/01/2024", 1150)]
        [InlineData("01/01/2025", 1150)]
        public void GivenLeagueYear_ThenReturnExpectedSalaryCap(string dateString, int expectedSalaryCap)
        {
            var date = DateOnly.Parse(dateString);

            CapSpaceUtilities.GetCurrentSalaryCap(date).Should().Be(expectedSalaryCap);
        }
    }
}