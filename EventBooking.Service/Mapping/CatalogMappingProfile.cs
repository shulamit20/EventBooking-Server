using AutoMapper;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;

namespace EventBooking.Service.Mapping;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // ---- entity -> response ----

        CreateMap<Venue, VenueResponse>();

        CreateMap<Hall, HallResponse>()
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.Venue.Name));

        CreateMap<HallSlot, HallSlotResponse>()
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name))
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.Hall.Venue.Name))
            .ForMember(d => d.Shift, o => o.MapFrom(s => s.Shift.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<ExtraService, ExtraServiceResponse>()
            .ForMember(d => d.Pricing, o => o.MapFrom(s => s.Pricing.ToString()))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.ServiceCategory.Name));

        CreateMap<EventType, EventTypeResponse>()
            .ForMember(d => d.ServiceCategoryIds,
                o => o.MapFrom(s => s.ServiceCategories.Select(x => x.ServiceCategoryId)));

        CreateMap<ServiceCategory, ServiceCategoryResponse>();

        // ---- request -> entity ----
        // Ids are database-generated; navigation properties, status/version and the owner are
        // set by the service or by defaults, so they are explicitly ignored.

        CreateMap<CreateVenueRequest, Venue>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.Halls, o => o.Ignore());

        CreateMap<UpdateVenueRequest, Venue>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.Halls, o => o.Ignore());

        CreateMap<CreateHallRequest, Hall>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Venue, o => o.Ignore())
            .ForMember(d => d.Slots, o => o.Ignore());

        CreateMap<CreateHallSlotRequest, HallSlot>()  // Shift is already the ShiftType enum
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Hall, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Version, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore());

        CreateMap<CreateExtraServiceRequest, ExtraService>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.ServiceCategory, o => o.Ignore())
            .ForMember(d => d.BookingExtraServices, o => o.Ignore());

        CreateMap<UpdateExtraServiceRequest, ExtraService>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.ServiceCategory, o => o.Ignore())
            .ForMember(d => d.BookingExtraServices, o => o.Ignore());
    }
}
