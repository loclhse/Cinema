using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RoomLayout : BaseEntity
    {
        public Guid CinemaRoomId { get; set; } // FK tới phòng

        // JsonDocument does not have a parameterless constructor. Use JsonDocument.Parse to initialize it.
        public JsonDocument LayoutJson { get; set; } = JsonDocument.Parse("{}"); // Bố cục layout thiết kế

        public virtual CinemaRoom? CinemaRoom { get; set; }
    }
}
