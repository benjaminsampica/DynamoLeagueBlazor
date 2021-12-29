using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities
{
    [TestFixture]
    internal class CapSpaceTest
    {
        [TestCase(0, 0, 0, ExpectedResult = 1050, Description = "Team has no players at all.")]
        [TestCase(500, 0, 0, ExpectedResult = 550, Description = "Team has rostered players with a total value of 500.")]
        [TestCase(0, 200, 0, ExpectedResult = 950, Description = "Team has unrostered players with a total value of 200.")]
        [TestCase(0, 0, 100, ExpectedResult = 950, Description = "Team has unsigned players with a total value of 100.")]
        [TestCase(600, 200, 100, ExpectedResult = 250, Description = "Team has rostered, unrostered, and unsigned players with a total value of 900.")]
        public int GivenDifferentPlayerValuesForToday_ThenReturnsExpectedRemainingCapSpace(int rostered, int unrostered, int dropped)
        {
            var today = new DateOnly(2021, 01, 01);

            return CapSpaceUtilities.GetRemainingCapSpace(today, rostered, unrostered, dropped);
        }

        [TestCase("01/01/2021", ExpectedResult = 1050, Description = "2021 Salary Cap")]
        [TestCase("01/01/2022", ExpectedResult = 1100, Description = "2022 Salary Cap")]
        [TestCase("01/01/2023", ExpectedResult = 1100, Description = "2023 Salary Cap")]
        [TestCase("01/01/2024", ExpectedResult = 1150, Description = "2024 Salary Cap")]
        [TestCase("01/01/2025", ExpectedResult = 1150, Description = "2025 Salary Cap")]
        public int GivenLeagueYear_ThenReturnExpectedSalaryCap(string dateString)
        {
            var date = DateOnly.Parse(dateString);
 
            return CapSpaceUtilities.GetCurrentSalaryCap(date);
        }
    }
}