using AutoMapper;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;

namespace EventBooking.Service.Mapping;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        // Assumes the booking was loaded with HallSlot -> Hall -> Venue and the extra-service lines.
        CreateMap<Booking, BookingResponse>()
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.HallSlot.Hall.Name))
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.HallSlot.Hall.Venue.Name))
            .ForMember(d => d.Date, o => o.MapFrom(s => s.HallSlot.Date))
            .ForMember(d => d.Shift, o => o.MapFrom(s => s.HallSlot.Shift.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ExtraServices, o => o.MapFrom(s => s.BookingExtraServices))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s =>
                s.HallSlot.BasePrice + s.BookingExtraServices.Sum(x => x.Quantity * x.PriceAtBooking)));

        CreateMap<BookingExtraService, BookingExtraServiceResponse>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.ExtraService.Name));
        // LineTotal is a computed getter on the DTO - nothing to map.
    }
}
