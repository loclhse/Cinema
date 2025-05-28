using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class CustomerScore
{
    public int ScoreId { get; set; }

    public int? UserId { get; set; }

    public int? TotalScore { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User? User { get; set; }
}
