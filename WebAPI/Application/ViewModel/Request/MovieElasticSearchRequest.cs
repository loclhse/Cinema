using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class MovieElasticSearchRequest
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Director { get; set; }
        public string? Rated { get; set; }
        public List<string>? GenreNames { get; set; }
    }
}
