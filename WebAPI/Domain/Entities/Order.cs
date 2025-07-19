using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class Order : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? OrderTime { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? TotalBonusPoint { get; set; }
    public OrderEnum? Status {  get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<SeatSchedule>? SeatSchedules { get; set; } = new List<SeatSchedule>();
    public virtual ICollection<SeatScheduleLog> SeatScheduleLogs { get; set; } = new List<SeatScheduleLog>();
    public virtual ICollection<SnackOrder> SnackOrders { get; set; } = new List<SnackOrder>();
    public virtual Subscription? Subscription { get; set; }
    public virtual AppUser? User { get; set; }
}
