using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.ViewModel.Request
{
    public class CinemaRoomUpdateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        // Tổng số hàng và cột ghế
        [Required]
        [MinLength(1, ErrorMessage = "TotalRows must be at least 1.")]
        [MaxLength(15, ErrorMessage = "TotalRows cannot exceed 15.")]
        public int TotalRows { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "TotalCols must be at least 1.")]
        [MaxLength(20, ErrorMessage = "TotalCols cannot exceed 20.")]
        public int TotalCols { get; set; }
    }
}