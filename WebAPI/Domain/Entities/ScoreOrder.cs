using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ScoreOrder : BaseEntity
    {
        public Guid RedeemId { get; set; }
        public Guid ScoreItemId { get; set; }
        public virtual ScoreItem? ScoreItem { get; set; }
        public virtual Redeem? Redeem { get; set; }
    }
}
