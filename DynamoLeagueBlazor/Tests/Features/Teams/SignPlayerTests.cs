using AutoBogus;
using DynamoLeagueBlazor.Server.Models;
using DynamoLeagueBlazor.Shared.Features.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoLeagueBlazor.Tests.Features.Teams
{
    public class SignPlayerTests : IntegrationTestBase
    {
        private const string _endpoint = "api/teams/signplayer";

        private static SignPlayerRequest CreateFakeValidRequest()
        {
            var faker = new AutoFaker<SignPlayerRequest>()
                .RuleFor(f => f.PlayerId, 1);

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

            var request = new SignPlayerRequest
            {
              PlayerId = int.MaxValue,
              YearContractExpires= DateTime.Now.Year
            };

            var client = application.CreateClient();

            var response = await client.PostAsJsonAsync(_endpoint, request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
