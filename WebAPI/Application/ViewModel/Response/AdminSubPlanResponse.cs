using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class AdminSubPlanResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public double Price { get; set; }
        public string? Imgs { get; set; }
        public string? Offer { get; set; }
        public int Users { get; set; } = 0;
        public PlanStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
