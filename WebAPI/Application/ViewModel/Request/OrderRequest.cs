using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class OrderRequest
    {
        [Required(AllowEmptyStrings = true)]
        public Guid? UserId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        [Required(AllowEmptyStrings = true)]
        public Guid? PromotionId { get; set; }

        public List<Guid>? SeatScheduleId { get; set; }
        
        public List<SnackOrderRequest>? Snack { get; set; }
        public List<SnackComboOrderRequest>? SnackCombo {  get; set; }
    }
}
