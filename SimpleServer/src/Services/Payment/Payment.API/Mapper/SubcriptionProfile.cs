using System;
using AutoMapper;
using Payment.API.Enum;

namespace Payment.API.Mapper
{
    public class SubcriptionProfile : Profile
    {
        public SubcriptionProfile()
        {
            CreateMap<Entity.Subcription, Model.Subcriptions>()
                // map id from source to Id in destination
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                // map string value of plantypeEnum from source to PlanType in destination
                .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src =>src.Plan.ToString()))
                // map price from source to $"{price}$" in destination
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => $"{src.Price}$"))
                // map string value of videoqualityEnum from source to VideoQuality in destination
                .ForMember(dest => dest.VideoQuality, opt => opt.MapFrom(src => src.VideoQuality.ToString()))
                // map resolution from source to Resolution in destination
                .ForMember(dest => dest.Resolution, opt => opt.MapFrom(src => src.Resolution))
                // map all device name from source to list of Devices in destination
                .ForMember(dest => dest.Devices, opt => opt.MapFrom(src => src.Devices.Select(x => x.Name).ToList()));
        }
    }
}
