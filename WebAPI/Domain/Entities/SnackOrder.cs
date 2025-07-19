using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SnackOrder : BaseEntity
    {
        public Guid OrderId {  get; set; }

        public Guid SnackId { get; set; }

        public int Quantity { get; set; }

        public SnackOrderEnum SnackOrderEnum { get; set; }
        public virtual Order? Order { get; set; }

    }
}
