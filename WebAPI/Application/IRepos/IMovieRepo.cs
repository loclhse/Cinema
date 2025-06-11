using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IMovieRepo : IGenericRepo<Movie>
    {
        Task<IEnumerable<Movie>> SearchMoviesAsync(string? searchTerm, string? searchType = null, int? limit = 5);


    }
}
