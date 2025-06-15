using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MapperConfigs
{
    public class MovieMapping : Profile
    {
        public MovieMapping()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CreateMap<Movie, MovieResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.GenreNames, opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre.Name ?? "Unknown")))
             .ForMember(dest => dest.Language, opt => opt.MapFrom(src => (Language?)src.Language))
             .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))
             .ForMember(dest => dest.Img, opt => opt.MapFrom(src => src.Img))
             .ForMember(dest => dest.TrailerUrl, opt => opt.MapFrom(src => src.TrailerUrl))
             .ForMember(dest => dest.Rated, opt => opt.MapFrom(src => (Rated?)src.Rated))
             .ForMember(dest => dest.MovieStatus, opt => opt.MapFrom(src => (MovieStatus?)src.MovieStatus))
              .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.EndDate))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))


             .ReverseMap()

             .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
             .ForMember(dest => dest.Showtimes, opt => opt.Ignore());
#pragma warning restore CS8602 // Dereference of a possibly null reference.





        }
    }
}
