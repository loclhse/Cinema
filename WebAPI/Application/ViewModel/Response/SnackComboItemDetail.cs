using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SnackComboItemDetail
    {
        public Guid SnackId { get; set; }
        public string? SnackName { get; set; } 
        public int Quantity { get; set; }
    }
}
