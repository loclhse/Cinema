using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class PromotionRepo : GenericRepo<Promotion>, IPromotionRepo
    {
        public PromotionRepo(AppDbContext ctx) : base(ctx)
        {
        }

        public async Task<Promotion> GetPromotionById(Guid id)
        {
            var rs = await _db.Where(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();
            return rs;
        }


    }
}
