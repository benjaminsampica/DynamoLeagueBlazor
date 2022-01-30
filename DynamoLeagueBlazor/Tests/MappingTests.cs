using AutoMapper;

namespace DynamoLeagueBlazor.Tests;

public class MappingTests
{
    [Test]
    public void AllMappersMapToProperties()
    {
        var allProfiles = typeof(Server.Infrastructure.ApplicationDbContext).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Profile)))
            .Select(type => (Profile)Activator.CreateInstance(type)!);

        var mapperConfiguration = new MapperConfiguration(_ => _.AddProfiles(allProfiles));

        mapperConfiguration.AssertConfigurationIsValid();
    }
}
