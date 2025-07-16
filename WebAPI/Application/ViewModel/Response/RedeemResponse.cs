using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class RedeemResponse
    {
        public Guid Id { get; set; }
        public int TotalScore { get; set; }
        //public int Quantity { get; set; }
        public ScoreStatus status { get; set; }
        public List<string> ItemNames { get; set; }
    }

}
