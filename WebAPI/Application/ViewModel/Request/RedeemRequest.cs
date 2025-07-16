using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class RedeemRequest
    {
        public Guid ScoreItemId { get; set; }
        public int Quantity { get; set; } = 1; 
    }
}
