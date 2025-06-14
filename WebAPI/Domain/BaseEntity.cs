namespace Domain
{
        public class BaseEntity
        {
            public Guid Id { get; set; } = Guid.NewGuid(); // Tự động sinh ID nếu không được cung cấp
            public DateTime CreateDate { get; set; } = DateTime.UtcNow;
            public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
            public bool IsDeleted { get; set; } = false;
        }
}
