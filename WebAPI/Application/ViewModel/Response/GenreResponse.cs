using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class GenreResponse
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
    }
}
