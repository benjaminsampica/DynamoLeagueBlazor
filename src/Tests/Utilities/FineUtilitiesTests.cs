using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities;

public class FineUtilitiesTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 2)]
    [InlineData(100, 3)]
    [InlineData(int.MaxValue, 7)]
    public void GivenDifferentContractAmounts_ThenReturnsExpectedResult(int contractValue, int expectedFineAmount)
        => FineUtilities.CalculateFineAmount(contractValue).Should().Be(expectedFineAmount);
}
