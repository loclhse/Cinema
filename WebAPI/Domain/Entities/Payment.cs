using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities;

public partial class Payment : BaseEntity
{
    public PaymentMethod? PaymentMethod { get; set; }

    public DateTime? PaymentTime { get; set; }

    public decimal? AmountPaid { get; set; }

    public string? TransactionCode { get; set; }

    public PaymentStatus? Status { get; set; }

    public Guid? OrderId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Subscription? Subscription { get; set; }

    public Guid? userId { get; set; }

    public virtual AppUser? User { get; set; }
}


