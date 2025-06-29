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

        public async Task<IEnumerable<Promotion>> GetAllPromotion()
        {
            var rs = await _db.Where(p => p.IsDeleted == false).ToListAsync();
            return rs;
        }

        public async Task<Promotion> GetPromotionById(Guid? id)
        {
            var rs = await _db.Where(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();
#pragma warning disable CS8603 // Possible null reference return.
            return rs;
#pragma warning restore CS8603 // Possible null reference return.
        }


    }
}
