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
        public Guid? UserId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public Guid? PromotionId { get; set; }

        public List<Guid>? SeatScheduleId { get; set; }
        
        public ICollection<SnackOrderRequest> SnackOrders { get; set; } = new List<SnackOrderRequest>();
        public ICollection<SnackComboOrderRequest> SnackComboOrders { get; set; } = new List<SnackComboOrderRequest>();
    }
}
