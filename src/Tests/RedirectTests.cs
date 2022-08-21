using Microsoft.AspNetCore.Mvc.Testing;

namespace DynamoLeagueBlazor.Tests;

public class RedirectTests
{
    [Fact]
    public async Task GivenAnyRequest_WhenIsHttp_ThenIsRedirectedToHttps()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();

        var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
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
        var webApplicationFactory = new WebApplicationFactory<Program>();

        var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://test.com")
        });

        var response = await client.GetAsync(string.Empty);

        response.Headers.Location!.AbsoluteUri.Should().Contain("www");
    }
}
