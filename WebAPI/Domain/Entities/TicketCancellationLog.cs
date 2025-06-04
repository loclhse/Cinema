namespace Domain.Entities;

public partial class TicketCancellationLog : BaseEntity
{
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public Guid? UserId { get; set; }
    public virtual AppUser? User { get; set; }
}
