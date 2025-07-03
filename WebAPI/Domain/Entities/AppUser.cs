using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AppUser : BaseEntity
{
    //Khóa chính Shared-PK (Shared với ApplicationUser.Id)
    public string? Email { get; set; }

    public string? IdentityCard { get; set; }

    public DateOnly? Dob { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }
    public string? Avatar { get; set; }

    public Sex Sex { get; set; }
    public string? Assign { get; set; }
    public double Salary { get; set; }
    public string? Position { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    //public virtual CustomerScore? CustomerScore { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Redeem> Redeems { get; set; } = new List<Redeem>();

    //public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    //public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();

    //public virtual ICollection<Theater> Theaters { get; set; } = new List<Theater>();


}
