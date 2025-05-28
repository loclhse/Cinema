using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaymentTime { get; set; }

    public decimal? AmountPaid { get; set; }

    public string? TransactionCode { get; set; }

    public virtual Order? Order { get; set; }
}
