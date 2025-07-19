using Application.ViewModel.Request;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; } 
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? OrderTime { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAfter { get; set; }
        public OrderEnum? Status { get; set; }

        public List<Guid> SeatSchedules { get; set; } = new List<Guid>();
        public ICollection<SnackOrderResponse>? Snacks { get; set; } = new List<SnackOrderResponse>();

    }
}
