using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities
{
   public partial class CinemaRoom : BaseEntity
   {
        [Required]
        public string Name { get; set; } = string.Empty;

        // Tổng số hàng và cột ghế
        public int TotalRows { get; set; }
        public int TotalCols { get; set; }

        // Tùy chọn: JSON chứa cấu trúc bố trí phức tạp (nếu có lối đi, vị trí trống)
        public JsonObject LayoutJson { get; set; } = new JsonObject();

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
   }
}


