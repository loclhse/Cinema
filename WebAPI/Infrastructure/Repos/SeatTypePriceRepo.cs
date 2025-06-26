using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    public class SeatTypePriceRepo : GenericRepo<SeatTypePrice>, ISeatTypePriceRepo
    {
        private new readonly AppDbContext _context;
        public SeatTypePriceRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<SeatTypePrice>> GetAllAsync()
        {
            var result = _context.SeatTypePrices
                .AsNoTracking()
                .ToListAsync();
            return result;
        }

        public Task<SeatTypePrice?> GetByTypeAsync(SeatTypes type)
        {
            var query = _context.SeatTypePrices
                .FirstOrDefaultAsync(x => x.SeatType == type);
            if (query == null) throw new KeyNotFoundException($"SeatTypePrice for '{type}' not found.");
            return query;
        }
    }
}
