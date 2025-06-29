using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Subscription: BaseEntity 
    {
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public Guid? SubscriptionPlanId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.pending ; 
        public virtual AppUser? User { get; set; }
        public double? Price { get; set; }
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}

