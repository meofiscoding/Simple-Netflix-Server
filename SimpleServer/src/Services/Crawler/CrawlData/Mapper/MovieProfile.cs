using AutoMapper;
using CrawlData.Model;
using EventBus.Message.Events;

namespace CrawlData.Mapper
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<MovieItem, TransferMovieListEvent>()
                .ForMember(dest => dest.MovieName, opt => opt.MapFrom(src => src.MovieName))
                .ForMember(dest => dest.Poster, opt => opt.MapFrom(src => src.Poster))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StreamingUrls, opt => opt.MapFrom(src => src.StreamingUrls))
                .ForMember(dest => dest.UrlDetail, opt => opt.MapFrom(src => src.UrlDetail))
                .ForMember(dest => dest.MovieCategory, opt => opt.MapFrom(src => src.MovieCategory))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        }
    }
}
