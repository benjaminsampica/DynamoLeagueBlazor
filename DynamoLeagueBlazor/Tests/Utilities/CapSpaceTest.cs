using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Utilities;

namespace DynamoLeagueBlazor.Tests.Utilities
{
    [TestFixture]
    internal class CapSpaceTest
    {
        private Team? stubTeam;
        private int rosteredPlayersContractValue;
        private int unrosteredPlayersContractValue;
        private int unsignedPlayersContractValue;

         [SetUp]
         public void Init()
        {
            stubTeam = CreateFakeTeam();

            var mockRosteredPlayer = CreateFakePlayer();
            mockRosteredPlayer.TeamId = stubTeam.Id;
            mockRosteredPlayer.SetToRostered(DateTime.MaxValue, 1);
            mockRosteredPlayer.ContractValue = 100;
            stubTeam.Players.Add(mockRosteredPlayer);

            var mockRosteredPlayer2 = CreateFakePlayer();
            mockRosteredPlayer2.TeamId = stubTeam.Id;
            mockRosteredPlayer2.SetToRostered(DateTime.MaxValue, 1);
            mockRosteredPlayer2.ContractValue = 200;
            stubTeam.Players.Add(mockRosteredPlayer2);

            var mockRosteredPlayer3 = CreateFakePlayer();
            mockRosteredPlayer3.TeamId = stubTeam.Id;
            mockRosteredPlayer3.SetToRostered(DateTime.MaxValue, 1);
            mockRosteredPlayer3.ContractValue = 200;
            stubTeam.Players.Add(mockRosteredPlayer3);

            rosteredPlayersContractValue = stubTeam.Players.Where(p => p.Rostered).Sum(cv => cv.ContractValue);

            var mockUnrosteredPlayer = CreateFakePlayer();
            mockUnrosteredPlayer.TeamId = stubTeam.Id;
            mockUnrosteredPlayer.SetToUnrostered();
            mockUnrosteredPlayer.ContractValue = 100;
            stubTeam.Players.Add(mockUnrosteredPlayer);

            var mockUnrosteredPlayer2 = CreateFakePlayer();
            mockUnrosteredPlayer2.TeamId = stubTeam.Id;
            mockUnrosteredPlayer2.SetToUnrostered();
            mockUnrosteredPlayer2.ContractValue = 100;
            stubTeam.Players.Add(mockUnrosteredPlayer2);

            unrosteredPlayersContractValue = stubTeam.Players.Where(p => !p.Rostered).Sum(cv => cv.ContractValue);

            var mockUnsignedPlayer = CreateFakePlayer();
            mockUnsignedPlayer.TeamId = stubTeam.Id;
            mockUnsignedPlayer.SetToUnsigned();
            mockUnsignedPlayer.ContractValue = 40;
            stubTeam.Players.Add(mockUnsignedPlayer);

            var mockUnsignedPlayer2 = CreateFakePlayer();
            mockUnsignedPlayer2.TeamId = stubTeam.Id;
            mockUnsignedPlayer2.SetToUnsigned();
            mockUnsignedPlayer2.ContractValue = 10;
            stubTeam.Players.Add(mockUnsignedPlayer2);

            unsignedPlayersContractValue = stubTeam.Players.Where(p => (!p.Rostered) && (p.ContractLength is null) && (p.YearAcquired == DateTime.Today.Year)).Sum(cv => cv.ContractValue);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2020_ThenReturn1050()
        {
            var date = new DateOnly(2020, 01, 01);
            var expectedResult = 1050;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2021_ThenReturn1050()
        {
            var date = new DateOnly(2021, 01, 01);
            var expectedResult = 1050;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2022_ThenReturn1100()
        {
            var date = new DateOnly(2022, 01, 01);
            var expectedResult = 1100;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2023_ThenReturn1100()
        {
            var date = new DateOnly(2023, 01, 01);
            var expectedResult = 1100;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2050_ThenReturn1800()
        {
            var date = new DateOnly(2050, 01, 01);
            var expectedResult = 1800;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenCurrentSalaryCap_WhenYearIs2051_ThenReturn1800()
        {
            var date = new DateOnly(2051, 01, 01);
            var expectedResult = 1800;
            var result = CapSpaceUtilities.GetCurrentCapValue(date);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenRosteredPlayers_WhenTheYearIs2021AndRosteredContractIs500_ThenReturnRemaingCapOf550()
        {
            var date = new DateOnly(2021, 01, 01);

            var expectedResult = 550;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, 0, 0);
            result.Should().Be(expectedResult);
        }
        [Test]
        public void GivenRosteredAndUnrosteredPlayers_WhenTheYearIs2021AndRosteredContractIs500AndUnrosteredContractIs200_ThenReturnRemaingCapOf450()
        {
            var date = new DateOnly(2021, 01, 01);

            var expectedResult = 450;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, unrosteredPlayersContractValue, 0);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenAllPlayerTypes_WhenTheYearIs2021AndRosteredContractIs500AndUnrosteredContractIs200AndUnsignedContractIs50_ThenReturnRemaingCapOf400()
        {
            var date = new DateOnly(2021, 01, 01);

            var expectedResult = 400;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, unrosteredPlayersContractValue, unsignedPlayersContractValue);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenRosteredPlayers_WhenTheYearIs2022AndRosteredContractIs500_ThenReturnRemaingCapOf600()
        {
            var date = new DateOnly(2022, 01, 01);

            var expectedResult = 600;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, 0, 0);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenRosteredAndUnrosteredPlayers_WhenTheYearIs2022AndRosteredContractIs500AndUnrosteredContractIs200_ThenReturnRemaingCapOf500()
        {
            var date = new DateOnly(2022, 01, 01);

            var expectedResult = 500;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, unrosteredPlayersContractValue, 0);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void GivenAllPlayerTypes_WhenTheYearIs2022AndRosteredContractIs500AndUnrosteredContractIs200AndUnsignedContractIs50_ThenReturnRemaingCapOf450()
        {
            var date = new DateOnly(2022, 01, 01);

            var expectedResult = 450;
            var result = CapSpaceUtilities.CalculateCurrentCapSpace(date, rosteredPlayersContractValue, unrosteredPlayersContractValue, unsignedPlayersContractValue);
            result.Should().Be(expectedResult);
        }
    }
}
