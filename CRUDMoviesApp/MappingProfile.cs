using AutoMapper;
using CRUDMoviesApp.Models;
using CRUDMoviesApp.ViewModels;

namespace CRUDMoviesApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap< src , dest>
            CreateMap<MovieFormViewModel, Movie>().ReverseMap();
            
            // .ForMember(dest => dest.Id , src => src.MapFrom(src => src.MovieId))
        }
    }
}
