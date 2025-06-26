using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SubscriptionPlanResponse
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public double Price { get; set; }
    }
}
