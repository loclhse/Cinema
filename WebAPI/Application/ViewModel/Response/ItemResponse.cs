using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class ItemResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Score { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
    }
}
