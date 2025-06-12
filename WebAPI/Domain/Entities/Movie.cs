using Domain.Enums;


namespace Domain.Entities;

public partial class Movie : BaseEntity
{
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; }
        public string? Img { get; set; }
        public Language Language { get; set; } 
        public string? TrailerUrl { get; set; }
        public Rated Rated { get; set; }
        public MovieStatus MovieStatus { get; set; } 
        public DateOnly? ReleaseDate { get; set; } 
        public DateOnly? EndDate { get; set; }
  
    public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
  


}
