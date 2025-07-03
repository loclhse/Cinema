using Application.ViewModel.Request;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IElasticMovieRepo
    {
        Task<bool> IndexMovieAsync(Movie movie);
        Task<IEnumerable<MovieElasticSearchRequest>> elasticSearchMoviesAsync(string keyword, int limit = 5);
    }
}
