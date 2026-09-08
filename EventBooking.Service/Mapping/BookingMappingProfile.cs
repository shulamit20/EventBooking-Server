using AutoMapper;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;

namespace EventBooking.Service.Mapping;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        // Assumes the booking was loaded with EventType, its (optional) HallSlot -> Hall -> Venue,
        // and the extra-service lines -> service -> category.
        CreateMap<Booking, BookingResponse>()
            .ForMember(d => d.EventTypeName, o => o.MapFrom(s => s.EventType.Name))
            .ForMember(d => d.CateringMenuName, o => o.MapFrom(s => s.CateringMenu != null ? s.CateringMenu.Name : null))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.HallSlot.Hall.Name))
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.HallSlot.Hall.Venue.Name))
            .ForMember(d => d.Date, o => o.MapFrom(s => s.HallSlot.Date))
            .ForMember(d => d.Shift, o => o.MapFrom(s => s.HallSlot.Shift.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ExtraServices, o => o.MapFrom(s => s.BookingExtraServices));
        // TotalPrice is a stored column — mapped by convention.

        CreateMap<BookingExtraService, BookingExtraServiceResponse>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.ExtraService.Name))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.ExtraService.ServiceCategory.Name));
        // Quantity, PriceAtBooking, LineTotal are mapped by convention.
    }
}
