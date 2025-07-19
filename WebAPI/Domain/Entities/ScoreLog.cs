using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class ScoreLog : BaseEntity
{
    public Guid UserId { get; set; }              // Bắt buộc có UserId
    public int PointsChanged { get; set; }        // Điểm được cộng/trừ
    public string? ActionType { get; set; } 

    public virtual AppUser? AppUser { get; set; } // Navigation đến người dùng
}
