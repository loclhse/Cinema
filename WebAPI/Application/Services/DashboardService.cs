using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ApiResp> GetDashboardDataAsync()
        {
            try
            {
                var dashboardData = new DashboardResponse();

                // Get movie rankings
                var movieRankings = await GetMovieRankingsDataAsync();
                dashboardData.MovieRankings = movieRankings;

                return new ApiResp().SetOk(dashboardData);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting dashboard data: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetMovieRankingsAsync(int? limit = null)
        {
            try
            {
                var movieRankings = await GetMovieRankingsDataAsync(limit);
                return new ApiResp().SetOk(movieRankings);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting movie rankings: {ex.Message}");
            }
        }

        public async Task<List<MovieRanking>> GetMovieRankingsDataAsync(int? limit = null)
        {
            // Get all successful payments
            var payments = await _uow.PaymentRepo.GetAllAsync(
                p => p.Status == PaymentStatus.Success && !p.IsDeleted,
                include: q => q.Include(p => p.Order)
            );

            // Get all seat schedules with showtimes and movies
            var seatSchedules = await _uow.SeatScheduleRepo.GetAllAsync(
                ss => payments.Select(p => p.OrderId).Contains(ss.OrderId.Value),
                include: q => q.Include(ss => ss.Showtime).ThenInclude(s => s.Movie)
            );

            // Group by movie and calculate total revenue from payments
            var movieRankings = seatSchedules
                .Where(ss => ss.Showtime?.Movie != null && !ss.Showtime.Movie.IsDeleted)
                .GroupBy(ss => new { ss.Showtime.Movie.Id, ss.Showtime.Movie.Title, ss.Showtime.Movie.Img })
                .Select(g => new MovieRanking
                {
                    MovieId = g.Key.Id,
                    MovieName = g.Key.Title ?? "Unknown Movie",
                    Poster = g.Key.Img ?? "",
                    TotalRevenue = g.Sum(ss => 
                        payments.FirstOrDefault(p => p.OrderId == ss.OrderId)?.AmountPaid ?? 0),
                    TotalTicketsSold = g.Count(),
                    AverageTicketPrice = g.Average(ss => 
                        payments.FirstOrDefault(p => p.OrderId == ss.OrderId)?.AmountPaid ?? 0)
                })
                .Where(mr => mr.TotalRevenue > 0)
                .OrderByDescending(x => x.TotalRevenue)
                .Take(limit ?? 10)
                .ToList();

            // Assign ranks based on revenue (highest revenue = rank 1)
            for (int i = 0; i < movieRankings.Count; i++)
            {
                movieRankings[i].Rank = i + 1;
            }

            return movieRankings;
        }
    }
} 