﻿using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class SnackRequest
    {
        public string? Name { get; set; }
        public SnackType? Type { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Description { get; set; }
        public SnackStatus SnackStatus { get; set; }
    }
}
