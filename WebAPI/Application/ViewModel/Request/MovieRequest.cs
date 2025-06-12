using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class MovieRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; }
        public string? Genre { get; set; }
        public string? Img { get; set; }
        public Language Language { get; set; }
        public string? TrailerUrl { get; set; }
        public Rated Rated { get; set; }
        public MovieStatus MovieStatus { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public List<Guid>? GenreIds { get; set; }
    }
}
