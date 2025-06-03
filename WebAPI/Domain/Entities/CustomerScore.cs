using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CustomerScore : BaseEntity
{
    public int? TotalScore { get; set; }

    public Guid? UserId { get; set; }
    public virtual AppUser? User { get; set; }
}
