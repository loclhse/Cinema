using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class OrderResponse
    {
        public Guid? UserId { get; set; }
        public DateTime? OrderTime { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? TotalBonusPoint { get; set; }
        public string? PaymentLink { get; set; }
        public List<SeatScheduleForOrderResponse> SeatSchedules { get; set; } = new List<SeatScheduleForOrderResponse>();
        //response them List Snack
    }
}
