namespace Infrastructure.Entities;

public partial class TicketCancellationLog : BaseEntity
{
    public int? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public int? UserId { get; set; }
    public virtual User? User { get; set; }
}
