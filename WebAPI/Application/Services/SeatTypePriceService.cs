using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IServices;
using Domain.Entities;

namespace Application.Services
{
    public class SeatTypePriceService : ISeatTypePriceService
    {
        public Task<List<SeatTypePrice>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SeatTypePrice> UpdatePriceAsync(object dto)
        {
            throw new NotImplementedException();
        }
    }
}
