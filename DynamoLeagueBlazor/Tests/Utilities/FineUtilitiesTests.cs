using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities;

internal class FineUtilitiesTests
{
    [TestCase(0, ExpectedResult = 1, Description = "Anything less than 1 returns 1.")]
    [TestCase(50, ExpectedResult = 2, Description = "Given a contract value of 50 the fine amount increases by 1 to a total of 2.")]
    [TestCase(100, ExpectedResult = 3, Description = "Given a contract value of 100 the fine amount increases by 2 to a total of 3.")]
    [TestCase(int.MaxValue, ExpectedResult = 7, Description = "The maximum fine amount can only be 7.")]
    public int GivenDifferentContractAmounts_ThenReturnsExpectedResult(int contractValue)
        => FineUtilities.CalculateFineAmount(contractValue);
}
