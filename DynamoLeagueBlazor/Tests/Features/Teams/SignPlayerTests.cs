using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Enums;
using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Tests.Features.Teams
{
    public class SignPlayerTests : IntegrationTestBase
    {
        private const string _endpoint = "api/teams/signplayer";

        private static SignPlayerRequest CreateFakeValidRequest()
        {
            var faker = new AutoFaker<SignPlayerRequest>();
            return faker.Generate();
        }

        [Fact]
        public async Task GivenUnauthenticatedUser_ThenDoesNotAllowAccess()
        {
            var application = CreateUnauthenticatedApplication();

            var client = application.CreateClient();

            var stubRequest = CreateFakeValidRequest();
            var response = await client.PostAsJsonAsync(_endpoint, stubRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GivenAuthenticatedAdmin_ThenSignsPlayer()
        {
            var application = CreateAdminAuthenticatedApplication();
            var player = CreateFakePlayer();
            player.Position = Position.QuarterBack.Name;
            player.YearContractExpires = DateTime.Now.Year;
            await application.AddAsync(player);
            var request = CreateFakeValidRequest();
            request.YearContractExpires = (int)player.YearContractExpires;
            request.PlayerId = player.Id;
            var client = application.CreateClient();

            await client.PostAsJsonAsync(_endpoint, request);

            var result = await application.FirstOrDefaultAsync<Player>();

            result.Should().NotBeNull();
            result!.YearContractExpires.Should().Be(request.YearContractExpires);
            result.Rostered.Should().BeTrue();
            result.EndOfFreeAgency.Should().BeNull();
            var position = Position.FromName(player.Position);
            var expectedContractValue = position.GetContractValue(player.ContractValue, request.YearContractExpires);
            result.ContractValue.Should().Be(expectedContractValue);
        }
    }
    public class SignPlayereRequestValidatorTests
    {
        private readonly SignPlayerRequestValidator _validator = null!;

        public SignPlayereRequestValidatorTests()
        {
            _validator = new SignPlayerRequestValidator();
        }

        [Theory]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        public void GivenDifferentRequests_ThenReturnsExpectedResult(int playerId, int yearContractExpires, bool expectedResult)
        {
            var request = new SignPlayerRequest { PlayerId = playerId, YearContractExpires = yearContractExpires };

            var result = _validator.Validate(request);

            result.IsValid.Should().Be(expectedResult);
        }
    }
}
