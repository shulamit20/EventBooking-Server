using AutoMapper;
using EventBooking.Service.Mapping;

namespace EventBooking.Tests.Mapping;

public class MappingProfilesTests
{
    /// <summary>
    /// Every <c>CreateMap</c> must account for every destination member (mapped or explicitly
    /// ignored). This is the test the Step F log left as a TODO — it fails the build the moment a
    /// new DTO property is added without a matching mapping rule.
    /// </summary>
    [Fact]
    public void AllProfiles_HaveAValidConfiguration()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthMappingProfile>();
            cfg.AddProfile<CatalogMappingProfile>();
            cfg.AddProfile<BookingMappingProfile>();
            cfg.AddProfile<CateringMappingProfile>();
        });

        config.AssertConfigurationIsValid();
    }
}
