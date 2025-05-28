using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class TicketCancellationLog
{
    public int CancelId { get; set; }

    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? CancelTime { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
