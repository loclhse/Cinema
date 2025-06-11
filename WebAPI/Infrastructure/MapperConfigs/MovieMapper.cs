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
            CreateMap<Movie, MovieResponse>()
             .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre.Name ?? "Unknown")))
             .ForMember(dest => dest.Language, opt => opt.MapFrom(src => (Language?)src.Language))
             .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))
             .ForMember(dest => dest.Img, opt => opt.MapFrom(src => src.Img))
             .ForMember(dest => dest.TrailerUrl, opt => opt.MapFrom(src => src.TrailerUrl))
             .ForMember(dest => dest.Rated, opt => opt.MapFrom(src => (Rated?)src.Rated))
             .ForMember(dest => dest.MovieStatus, opt => opt.MapFrom(src => (MovieStatus?)src.MovieStatus))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
             .ForMember(dest => dest.Showtimes, opt => opt.MapFrom(src => src.Showtimes.Select(s => new ShowtimeResponse(s))))
             .ForMember(dest => dest.RecommendedMovies, opt => opt.MapFrom(src => src.RecommendedMovies))
             .ReverseMap()
             .ForMember(dest => dest.MovieGenres, opt => opt.Ignore()) // Ignore in reverse mapping
             .ForMember(dest => dest.Showtimes, opt => opt.Ignore())
            .ForMember(dest => dest.RecommendedMovies, opt => opt.Ignore());



            CreateMap<Showtime, ShowtimeResponse>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                 .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ReverseMap();
        }
    }
}
