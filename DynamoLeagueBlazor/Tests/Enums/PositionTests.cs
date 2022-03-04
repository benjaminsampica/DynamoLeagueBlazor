using DynamoLeagueBlazor.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoLeagueBlazor.Tests.Enums
{
    public class PositionTests
    {
        [Theory]
        [InlineData("QB", 1,0,1)]
        [InlineData("QB", 1, 5, 150)]
        [InlineData("RB", 1, 0, 1)]
        [InlineData("RB", 1,5,300)]
        [InlineData("WR", 1, 0, 1)]
        [InlineData("WR", 1,5,150)]
        [InlineData("TE", 1, 0, 1)]
        [InlineData("TE", 1,5,145)]
        [InlineData("DEF", 1, 0, 1)]
        [InlineData("DEF", 1,5,6)]
        public void GivenBidValueAndPosition_ThenReturnsContractAmount(string positionValue,int bidValue,int chartElement,int minimumAmount)
        {
           var position = Position.FromName(positionValue);
           var contractOption = position.CalculateContractYearOptions(bidValue);
           contractOption.ElementAt(chartElement).MinimumAmount.Should().Be(minimumAmount);
        }
    }
}
