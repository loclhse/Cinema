using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class BaseEntity<T>
    {
        [Key]
        public T Id { get; set; } = default!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
