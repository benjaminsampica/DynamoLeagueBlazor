using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities
{
    internal class CapSpaceTest : IntegrationTestBase
    {
         
        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2020_ThenReturn1050()
        {
            var date = new DateOnly(2020, 01, 01);
            var expectedResult = 1050;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2021_ThenReturn1050()
        {
            var date = new DateOnly(2021, 01, 01);
            var expectedResult = 1050;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2022_ThenReturn1100()
        {
            var date = new DateOnly(2022, 01, 01);
            var expectedResult = 1100;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2023_ThenReturn1100()
        {
            var date = new DateOnly(2023, 01, 01);
            var expectedResult = 1100;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2050_ThenReturn1600()
        {
            var date = new DateOnly(2050, 01, 01);
            var expectedResult = 1600;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2051_ThenReturn1600()
        {
            var date = new DateOnly(2051, 01, 01);
            var expectedResult = 1600;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Equals(expectedResult);
        }

    }
}
