using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SnackOrderResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public Guid SnackId { get; set; }

        public int Quantity { get; set; }

        public SnackOrderEnum SnackOrderEnum { get; set; }
    }
}
