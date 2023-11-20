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
            CreateMap<MovieInformation, TransferMovieListEvent>().ReverseMap();
        }
    }
}
