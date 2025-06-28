using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class SnackOrderRequest
    {
        public Guid SnackId { get; set; }
        int Quantity { get; set; }
    }
}
