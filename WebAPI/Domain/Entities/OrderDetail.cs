using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class OrderDetail : BaseEntity
{
    public int? ReferenceId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? BonusPoint { get; set; }

    public int? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();
}
