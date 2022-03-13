using DynamoLeagueBlazor.Shared.Enums;

namespace DynamoLeagueBlazor.Tests.Enums
{
    public class PositionTests
    {
        [Theory]
        [InlineData("QB", 1, 0, 1)]
        [InlineData("QB", 1, 5, 150)]
        public void GivenBidValueAndPosition_ThenReturnsContractAmount(string positionValue, int bidValue, int chartElement, int minimumAmount)
        {
            var position = Position.FromName(positionValue);
            var contractOption = position.CalculateContractYearOptions(bidValue);
            contractOption.ElementAt(chartElement).MinimumAmount.Should().Be(minimumAmount);
        }
    }
}
