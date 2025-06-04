using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked => DateTime.UtcNow >= ExpiresAt || RevokedAt != null;
        public DateTime? RevokedAt { get; set; }
        public Guid UserId { get; set; }

        public AppUser AppUser { get; set; } = default!;
    }
}
