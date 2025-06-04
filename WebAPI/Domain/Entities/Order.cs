using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class Order : BaseEntity
{


    public DateTime? OrderTime { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? TotalBonusPoint { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<TicketCancellationLog> TicketCancellationLogs { get; set; } = new List<TicketCancellationLog>();

    public Guid? UserId { get; set; }
    public virtual AppUser? User { get; set; }
}
