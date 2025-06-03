using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class TicketCancellationLog:BaseEntity
{
   

    public Guid? OrderId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? CancelTime { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
