using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.ViewModel.Request
{
    public class SeatTypePriceUpdateRequest
    {
        /// <summary>
        /// Giá mới (mặc định) cho loại ghế đó
        /// </summary>
        public double NewPrice { get; set; }
    }
}