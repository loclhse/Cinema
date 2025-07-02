using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class SnackComboOrderRequest
    {
        public Guid SnackComboId { get; set; }
        public int Quantity { get; set; }
    }
}
