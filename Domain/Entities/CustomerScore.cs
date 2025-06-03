using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class CustomerScore:BaseEntity
{
    public Guid? UserId { get; set; }

    public int? TotalScore { get; set; }
    public virtual User? User { get; set; }
}
