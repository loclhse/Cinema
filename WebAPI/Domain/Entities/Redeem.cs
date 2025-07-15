using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Redeem : BaseEntity {
    public Guid UserId { get; set; } 
    public int TotalScore { get; set; }  
    public int Quantity { get; set; }
    public ScoreStatus status { get; set; } = ScoreStatus.panding;
        public virtual AppUser? User { get; set; } 
    public virtual ICollection<ScoreOrder> ScoreOrders { get; set; } = new List<ScoreOrder>();
    }
}
