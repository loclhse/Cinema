using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class ScoreLog : BaseEntity
{
    public Guid UserId { get; set; }              // Bắt buộc có UserId
    public Guid? MovieId { get; set; }
    public string? PointsChanged { get; set; } = null;      // Điểm được cộng/trừ
    public string? ActionType { get; set; }
    public string? ItemName { get; set; }
    public virtual AppUser? AppUser { get; set; } // Navigation đến người dùng
}
