using AutoMapper;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;

namespace EventBooking.Service.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
    }
}
