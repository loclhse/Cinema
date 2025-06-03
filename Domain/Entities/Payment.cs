using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Payment:BaseEntity 
{
    

    public Guid? OrderId { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaymentTime { get; set; }

    public decimal? AmountPaid { get; set; }

    public string? TransactionCode { get; set; }

    public virtual Order? Order { get; set; }
}
