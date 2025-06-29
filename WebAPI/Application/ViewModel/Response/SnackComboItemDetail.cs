using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.ViewModel.Response
{
    public class SnackComboItemDetail
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } 
        public int? Quantity { get; set; }
        public string? ImgUrl { get; set; }
        public decimal? Price { get; set; }
        public SnackType? Type { get; set; }
        public string? Description { get; set; }
       
    }
}
