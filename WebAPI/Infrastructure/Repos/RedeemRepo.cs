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
    public class RedeemRepo : GenericRepo<Redeem>, IRedeemRepo
    {
        public RedeemRepo(AppDbContext context) : base(context)
        {
        }
        public async Task<List<string>> GetItemNamesByRedeemId(Guid redeemId)
        {
            var redeem = await GetAsync(
                r => r.Id == redeemId && !r.IsDeleted,
                include: query => query.Include(r => r.ScoreOrders).ThenInclude(so => so.ScoreItem)
                );
            if (redeem == null)
            {
                throw new Exception("Redeem not found.");
            }
            var orderNames= redeem.ScoreOrders
                .Select(so => so.ScoreItem.Name)
                .ToList();
            return orderNames;
        }
    }
}
