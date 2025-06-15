using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.ViewModel.Response
{
    public class SeatTypePriceResponse
    {
        public Guid Id { get; set; }
        public SeatTypes SeatType { get; set; }
        public double DefaultPrice { get; set; }

    }
}