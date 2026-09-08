using AutoMapper;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;

namespace EventBooking.Service.Mapping;

public class CateringMappingProfile : Profile
{
    public CateringMappingProfile()
    {
        CreateMap<CateringMenu, CateringMenuResponse>();

        CreateMap<CreateCateringMenuRequest, CateringMenu>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore());

        CreateMap<UpdateCateringMenuRequest, CateringMenu>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerUserId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore());
    }
}
