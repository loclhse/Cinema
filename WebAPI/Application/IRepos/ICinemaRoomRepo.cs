using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.IRepos
{
    public interface ICinemaRoomRepo : IGenericRepo<CinemaRoom>
    {
        Task<bool> ExistsAsync(Guid roomId);
        Task<CinemaRoom?> GetWithSeatsAsync(Guid roomId);
        Task<List<CinemaRoom>> GetAllWithPagingAsync(int pageIndex, int pageSize);
    }
}
