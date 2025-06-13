using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.IServices
{
    public interface ISeatTypePriceService
    {
        Task<List<SeatTypePrice>> GetAllAsync();
        Task<SeatTypePrice> UpdatePriceAsync(object dto);
    }
}
