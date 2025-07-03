using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ScoreItem : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Score { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; } 
        public int Sold { get; set; } 
        public virtual  ICollection<ScoreOrder> ScoreOrders { get; set; } = new List<ScoreOrder>();
    }
}
