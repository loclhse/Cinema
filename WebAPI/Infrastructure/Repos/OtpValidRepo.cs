using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    public class OtpValidRepo : GenericRepo<OtpValid>, IOtpValidRepo
    {
        private new readonly AppDbContext _context;

        public OtpValidRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OtpValid?> GetValidOtpAsync(Guid userId, string pin)
        {
            return await _context.Set<OtpValid>()
                                 .Where(o => o.AppUserId == userId &&
                                             o.ResetPin == pin &&
                                             o.ExpiryTime >= DateTime.UtcNow)
                                 .FirstOrDefaultAsync();
        }

        public async Task RemoveAllByUserIdAsync(Guid userId)
        {
            var list = await _context.Set<OtpValid>()
                                     .Where(o => o.AppUserId == userId)
                                     .ToListAsync();
            if (list.Any())
            {
                _context.Set<OtpValid>().RemoveRange(list);
                await _context.SaveChangesAsync();
            }
        }
    }
}
