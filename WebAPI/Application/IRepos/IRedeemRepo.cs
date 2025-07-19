using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IRedeemRepo : IGenericRepo<Redeem>
    {
        Task<List<string>> GetItemNamesByRedeemId(Guid redeemId);
    }
}
