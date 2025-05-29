using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Promotion : BaseEntity
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? DiscountPercent { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
