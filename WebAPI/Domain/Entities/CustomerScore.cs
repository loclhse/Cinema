using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class CustomerScore : BaseEntity
{
    //public int ScoreId { get; set; }

    //public int? UserId { get; set; }

    //public int? TotalScore { get; set; }

    //public DateTime? LastUpdated { get; set; }

    public Guid? UserId { get; set; }
    public virtual AppUser? User { get; set; }
}
