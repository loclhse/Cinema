using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.IRepos
{
    public interface ISeatTypePriceRepo : IGenericRepo<SeatTypePrice>
    {
        Task<SeatTypePrice?> GetByTypeAsync(SeatTypes type);
        Task<List<SeatTypePrice>> GetAllAsync();
    }
}
