using Microsoft.AspNetCore.Mvc.Testing;

namespace DynamoLeagueBlazor.Tests;

public class RedirectTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenAnyRequest_WhenIsHttp_ThenIsRedirectedToHttps()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://www.test.com")
        });

        var response = await client.GetAsync(string.Empty);

        response.Headers.Location!.Scheme.Should().Be("https");
    }

    [Fact]
    public async Task GivenAnyRequest_WhenIsNotWww_ThenIsRedirectedToWww()
    {
        var application = GetUnauthenticatedApplication();

        var client = application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://test.com")
        });

        var response = await client.GetAsync(string.Empty);

        response.Headers.Location!.AbsoluteUri.Should().Contain("www");
    }
}
