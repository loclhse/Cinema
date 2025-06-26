using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class ShowtimeResquest
    {
        public DateTime StartTime { get; set; }
        public DateOnly Date { get; set; }
    }
}
