using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.ViewModel.Request
{
    public class CinemaRoomCreateRequest
    {
        [Required]
        [NotNull]
        public string Name { get; set; } = string.Empty;

        // Tổng số hàng và cột ghế
        [Required]
        [Range(1, 15, ErrorMessage = "TotalRows must be between 1 and 15.")]
        public int TotalRows { get; set; }
        
        [Required]
        [Range(1, 20, ErrorMessage = "TotalCols must be between 1 and 20.")]
        public int TotalCols { get; set; }

    }
}
