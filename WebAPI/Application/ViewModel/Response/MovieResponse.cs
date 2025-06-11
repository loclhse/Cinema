using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class MovieResponse
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; }
        public List<string>? Genres { get; set; } // Mapped from Genre.Name via MovieGenres
        public Language? Language { get; set; }
        public string? Img { get; set; }
        public string? TrailerUrl { get; set; }
        public Rated? Rated { get; set; }
        public MovieStatus? MovieStatus { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public List<ShowtimeResponse>? Showtimes { get; set; }

        public List<MovieResponse>? RecommendedMovies { get; set; }


    }
}
