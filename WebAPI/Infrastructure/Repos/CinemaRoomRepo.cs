using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class CinemaRoomRepo : GenericRepo<CinemaRoom>, ICinemaRoomRepo
    {
        private new readonly AppDbContext _context;
        public CinemaRoomRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid roomId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CinemaRoom>> GetAllWithPagingAsync(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<CinemaRoom?> GetWithSeatsAsync(Guid roomId)
        {
            throw new NotImplementedException();
        }
    }
}
