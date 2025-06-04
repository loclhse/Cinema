using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Domain
{
    public sealed class IdentityWithProfile
    {
        public DomainUser Identity { get; init; } = default!;
        public AppUser Profile { get; init; } = default!;
    }
}
