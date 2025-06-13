using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.ViewModel.Response
{
    public class CinemaRoomResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int TotalCols { get; set; }
        public string LayoutJson { get; set; } = "{}"; // Default to empty JSON object
    }
}