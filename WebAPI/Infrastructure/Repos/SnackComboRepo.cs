﻿using Application.IRepos;
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
    public class SnackComboRepo: GenericRepo<SnackCombo>, ISnackComboRepo
    {
        public SnackComboRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SnackCombo>> GetCombosWithSnacksAsync()
        {
            return await _db
                .Include(sc => sc.SnackComboItems)
                .ThenInclude(sci => sci.Snack)
                .Where(sc => !sc.IsDeleted && sc.SnackComboItems.Any(sci => !sci.IsDeleted))
                .ToListAsync();
        }

        public async Task<SnackCombo> GetComboWithItemsAsync(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _db
                .Include(sc => sc.SnackComboItems)
                .ThenInclude(sci => sci.Snack)
                .FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
#pragma warning restore CS8603 // Possible null reference return.
        }
      
        public async Task AddComboItemAsync(SnackComboItem comboItem)
        {
            await _context.Set<SnackComboItem>().AddAsync(comboItem);
        }

       

    }
}
