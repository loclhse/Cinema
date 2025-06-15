using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SeatMatrixResponse
    {
        public int TotalRows { get; set; }
        public int TotalCols { get; set; }
        public List<List<SeatResponse>> Seats2D { get; set; } = new();
    }
}
