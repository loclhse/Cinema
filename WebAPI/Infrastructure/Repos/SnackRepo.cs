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
    public class SnackRepo : GenericRepo<Snack>, ISnackRepo
    {
        public SnackRepo(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Snack>> GetSnacksInComboAsync(Guid comboId)
        {
            return await _db
                .Include(s => s.SnackComboItems)
                .Where(s => s.SnackComboItems.Any(sci => sci.ComboId == comboId && !sci.IsDeleted) && !s.IsDeleted)
                .ToListAsync();
        }
    }
    
    }

