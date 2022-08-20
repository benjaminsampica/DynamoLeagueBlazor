using Microsoft.AspNetCore.Mvc.Testing;

namespace DynamoLeagueBlazor.Tests;

public class RedirectTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAnyRequest_WhenIsHttp_ThenIsRedirectedToHttps()
    {
        var application = CreateUnauthenticatedApplication();

        var client = application.CreateDefaultClient();

        var response = await client.GetAsync(string.Empty);

        response.Should().HaveStatusCode(HttpStatusCode.Moved);
        response.Headers.Location!.Scheme.Should().Be("https");
    }

    [Fact]
    public async Task GivenAnyRequest_WhenIsNotWww_ThenIsRedirectedToWww()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();

        var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://23n4nbsajdnbajskdbaskjdbasjkdbasd.com")
        });

        var response = await client.GetAsync(string.Empty);

        response.Should().HaveStatusCode(HttpStatusCode.PermanentRedirect);
        response.Headers.Location!.AbsoluteUri.Should().Contain("www");
    }
}
