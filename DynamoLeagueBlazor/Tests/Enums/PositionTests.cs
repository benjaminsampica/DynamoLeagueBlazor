using DynamoLeagueBlazor.Shared.Enums;

namespace DynamoLeagueBlazor.Tests.Enums;

public class PositionTests
{
    [Theory]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 2)]
    public void GivenABidValueAndPosition_ThenReturnsExpectedMinimumAmount(int bidValue, int index, int expectedMinimumAmount)
    {
        var position = new FakePosition();
        var contractOptions = position.CalculateContractYearOptions(bidValue);

        contractOptions.ElementAt(index).MinimumAmount.Should().Be(expectedMinimumAmount);
    }

    [Fact]
    public void GivenAContractValueOfOneAndThisYear_WhenThePositionPriceTableIsLessThanTheContractValue_ThenReturnsTheContractValue()
    {
        var thisYear = DateTime.Today.Year;
        var position = new FakePosition();

        var result = position.GetContractValue(thisYear, 1);

        result.Should().Be(1);
    }

    [Fact]
    public void GivenAContractValueOfOneAndNextYear_WhenThePositionPriceTableAmountIsGreaterThanTheContractValue_ThenReturnsThePriceTableAmount()
    {
        var nextYear = DateTime.Today.AddYears(1).Year;
        var position = new FakePosition();

        var result = position.GetContractValue(nextYear, 1);

        result.Should().Be(2);
    }
}

internal class FakePosition : Position
{
    public FakePosition() : base(nameof(FakePosition), 0)
    {

    }

    public override int[] PerYearContractPriceTable() => new[] { 1, 2 };
}