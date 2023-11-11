using System;
using AutoMapper;
using EventBus.Message.Events;
using Movie.API.Models;

namespace Movie.API.Mapper
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<MovieInformation, TransferMovieListEvent>()
                // auto map all properties except the CountWatched property
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
