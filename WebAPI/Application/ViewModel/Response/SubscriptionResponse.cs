using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SubscriptionResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? SubscriptionPlanId { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? Price { get; set; }
        public SubscriptionStatus Status { get; set; }
    }
}
