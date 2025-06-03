namespace Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string? Assign { get; set; }
        public int? Salary { get; set; }

        public Guid? UserId { get; set; }
        public virtual AppUser? User { get; set; }
    }
}
