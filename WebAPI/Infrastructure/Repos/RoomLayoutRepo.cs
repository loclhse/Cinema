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
    public class RoomLayoutRepo : GenericRepo<RoomLayout>, IRoomLayoutRepo
    {
        private new readonly AppDbContext _context;
        public RoomLayoutRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
