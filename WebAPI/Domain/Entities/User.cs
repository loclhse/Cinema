using Domain.Entities;
using Infrastructure.Enums;

namespace Infrastructure.Entities;

public partial class User : BaseEntity
{

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public int? Identitycart { get; set; }

    public DateOnly? Dob { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public Sex Sex { get; set; }

    public Role role { get; set; }

    public virtual CustomerScore? CustomerScore { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<ScoreHistory> ScoreHistories { get; set; } = new List<ScoreHistory>();

    public virtual ICollection<Theater> Theaters { get; set; } = new List<Theater>();

    public virtual ICollection<TicketCancellationLog> TicketCancellationLogs { get; set; } = new List<TicketCancellationLog>();

    public virtual ICollection<Employee> Employee { get; set; } = new HashSet<Employee>();
}
