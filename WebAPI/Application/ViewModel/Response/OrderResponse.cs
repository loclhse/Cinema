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
        public Guid? UserId { get; set; } 
        public DateTime? OrderTime { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAfter { get; set; }
        public OrderEnum? Status { get; set; }

        public List<SeatScheduleForOrderResponse> SeatSchedules { get; set; } = new List<SeatScheduleForOrderResponse>();
        public List<SnackOrderRequest>? Snacks { get; set; } = new List<SnackOrderRequest>();
        public List<SnackComboOrderRequest>? SnackCombos { get; set; } = new List<SnackComboOrderRequest>();
    }
}
