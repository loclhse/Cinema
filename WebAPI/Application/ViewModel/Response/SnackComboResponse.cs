using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SnackComboResponse
    {
        
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public string? ImgUrl { get; set; }
            public string? Description { get; set; }
            public decimal? TotalPrice { get; set; }
            public decimal? Discount { get; set; }
            public SnackStatus SnackComboStatus { get; set; }
        public virtual ICollection<SnackComboItemDetail>? Items { get; set; }
    }
    }

