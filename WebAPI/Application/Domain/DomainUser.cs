using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Domain
{
    /// <summary>
    /// DomainUser chỉ chứa những trường cần thiết để tạo một IdentityUser
    /// (ApplicationUser) trong tầng Infrastructure, đồng thời tách biệt khỏi lớp
    /// ApplicationUser (IdentityUser<Guid>).
    /// </summary>
    public class DomainUser
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
