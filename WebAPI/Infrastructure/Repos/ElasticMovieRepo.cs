using Application.IRepos;
using Application.ViewModel.Request;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class ElasticMovieRepo : IElasticMovieRepo
    {
        private readonly ElasticsearchClient _elasticClient;
        public ElasticMovieRepo(ElasticsearchClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        //Elastic Search
        public async Task<IEnumerable<MovieElasticSearchRequest>> elasticSearchMoviesAsync(string keyword, int? limit = 5)
        {
            var searchResponse = await _elasticClient.SearchAsync<MovieElasticSearchRequest>(s => s
                .Indices("movies")
                .Query(q => q
                    .MultiMatch(m => m
                    .Query(keyword)
                        .Fields(new[] { "title^3", "director", "rated", "genreNames^2" })
                    .Fuzziness(new Fuzziness("AUTO")) 
                    .MaxExpansions(10) 
                    )
                )
                .Size(limit)
            );
            return searchResponse.Documents;
        }
        public async Task<bool> IndexMovieAsync(Movie movie)
        {
            var index = new MovieElasticSearchRequest
            {
                Id = movie.Id,
                Title = movie.Title,
                Director = movie.Director,
                Rated = movie.Rated.ToString(),
                GenreNames = movie.MovieGenres?.Select(g => g.Genre?.Name ?? "").ToList()
            };
            var response = await _elasticClient.IndexAsync(index, i => i
                .Index("movies")
                .Id(index.Id.ToString())
            );

            return response.IsValidResponse;
        }
    }
}
