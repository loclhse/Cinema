using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Movie : BaseEntity
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Duration { get; set; }

    public string? Rating { get; set; }

    public string? Genre { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
