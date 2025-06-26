using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class EditPromotionRequest
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal? DiscountPercent { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
