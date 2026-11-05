using AutoMapper;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Entities;

namespace CottageBookingSystem.Service;

public sealed class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Cottage, CottageDto>()
            .ForMember(dto => dto.Amenities, options => options.MapFrom(cottage => cottage.Amenities.Select(amenity => amenity.Name)));

        CreateMap<CreateBookingDto, CottageBooking>();
        CreateMap<CottageBooking, BookingResponseDto>();
    }
}
