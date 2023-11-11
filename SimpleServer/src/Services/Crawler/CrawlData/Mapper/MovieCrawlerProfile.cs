using AutoMapper;
using CrawlData.Model;
using EventBus.Message.Events;

namespace CrawlData.Mapper
{
    public class MovieCrawlerProfile : Profile
    {
        public MovieCrawlerProfile()
        {
            CreateMap<MovieItem, TransferMovieListEvent>()
                // map all non null or ampty element from StreamingUrls to StreamingUrls
                .ForMember(dest => dest.StreamingUrls, opt => opt.MapFrom(src => src.StreamingUrls.Where(x => !string.IsNullOrEmpty(x.Value))))
                .ForMember(dest => dest.MovieName, opt => opt.MapFrom(src => src.MovieName))
                .ForMember(dest => dest.Poster, opt => opt.MapFrom(src => src.Poster))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.MovieCategory, opt => opt.MapFrom(src => src.MovieCategory))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        }
    }
}
