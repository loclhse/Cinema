using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;

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
            throw new NotImplementedException();
        }

        public Task<SeatTypePrice?> GetByTypeAsync(SeatTypes type)
        {
            throw new NotImplementedException();
        }
    }
}
