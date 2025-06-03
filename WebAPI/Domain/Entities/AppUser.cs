using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AppUser : BaseEntity<Guid>
{
    //Khóa chính Shared-PK (Shared với ApplicationUser.Id)

    public string FullName { get; set; } = string.Empty;
    public DateOnly? Dob { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Sex Sex { get; set; }
    public int? IdentityCard { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    //public virtual CustomerScore? CustomerScore { get; set; }

    //public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    //public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    //public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();

    //public virtual ICollection<Theater> Theaters { get; set; } = new List<Theater>();

    //public virtual ICollection<TicketCancellationLog> TicketCancellationLogs { get; set; } = new List<TicketCancellationLog>();
}
