using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class ScoreHistory:BaseEntity
{
    

    public Guid? UserId { get; set; }

    public Guid? TicketId { get; set; }

    public int? Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrderDetail? Ticket { get; set; }

    public virtual User? User { get; set; }
}
