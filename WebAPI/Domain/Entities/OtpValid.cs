using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OtpValid : BaseEntity
    {
        [Required]
        public Guid AppUserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string ResetPin { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryTime { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
    }
}
