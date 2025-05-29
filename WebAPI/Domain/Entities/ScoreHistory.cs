using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class ScoreHistory : BaseEntity
{
    public int? Amount { get; set; }

    public string? Description { get; set; }

    public int? TicketId { get; set; }
    public virtual OrderDetail? Ticket { get; set; }

    public int? UserId { get; set; }
    public virtual User? User { get; set; }
}
