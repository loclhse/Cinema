using System;
using System.Collections.Generic;

namespace Application.ViewModel.Response
{
    public class DashboardResponse
    {
        public RevenueAnalytics RevenueAnalytics { get; set; } = new();
        public List<MovieRanking> MovieRankings { get; set; } = new();
    }

    public class RevenueAnalytics
    {
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
        public List<DailyRevenue> DailyRevenues { get; set; } = new();
        public List<WeeklyRevenue> WeeklyRevenues { get; set; } = new();
        public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
    }

    public class WeeklyRevenue
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int TicketCount { get; set; }
    }

    public class MovieRanking
    {
        public Guid MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public int Rank { get; set; }
        public decimal AverageTicketPrice { get; set; }
    }
} 