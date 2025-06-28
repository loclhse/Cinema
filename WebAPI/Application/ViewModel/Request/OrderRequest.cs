using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class OrderRequest
    {
        public Guid? UserId { get; set; }

        public string? PaymentMethod { get; set; }

        public List<Guid>? SeatScheduleId { get; set; }
        
        public List<SnackOrderRequest>? Snack { get; set; }
        public List<SnackComboOrderRequest>? SnackCombo {  get; set; }
    }
}
