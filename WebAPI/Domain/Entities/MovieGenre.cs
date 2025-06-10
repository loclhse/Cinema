using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MovieGenre : BaseEntity
    {
        public Guid MovieId { get; set; }
        public Guid GenreId { get; set; }
        public virtual Genre? Genre { get; set; }
        public virtual Movie? Movie { get; set; }
    }
}
