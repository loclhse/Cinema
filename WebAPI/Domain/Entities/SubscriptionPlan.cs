using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; } 
        public double Price { get; set; }
        public string? Imgs { get; set; }
        public string? Offer { get; set; }
        public int Users { get; set; } = 0;
        public PlanStatus Status { get; set; } = PlanStatus.Inactive; // Mặc định là InActive
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
