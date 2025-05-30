namespace Domain.Entities;

public partial class CinemaRoom : BaseEntity
{
    public string? Name { get; set; }

    public int? Capacity { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public int? TheaterId { get; set; }
    public virtual Theater? Theater { get; set; }
}
