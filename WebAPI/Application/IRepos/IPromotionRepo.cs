using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IPromotionRepo : IGenericRepo<Promotion>
    {
        Task<Promotion> GetPromotionById(Guid id);
    }
}
