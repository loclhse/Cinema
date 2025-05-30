namespace Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string? Assign { get; set; }
        public int? Salary { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
