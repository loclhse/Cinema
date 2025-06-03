using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Theater:BaseEntity
{
    public int TheaterId { get; set; }
    
    public Guid? UserId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public int? ManagerId { get; set; }

    public virtual ICollection<CinemaRoom> CinemaRooms { get; set; } = new List<CinemaRoom>();

    public virtual User? User { get; set; }
}
