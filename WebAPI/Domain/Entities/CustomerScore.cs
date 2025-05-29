using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class CustomerScore : BaseEntity
{
    public int? TotalScore { get; set; }

    public int? UserId { get; set; }
    public virtual User? User { get; set; }
}
