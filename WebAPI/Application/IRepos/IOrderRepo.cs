using Application.ViewModel.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IOrderRepo : IGenericRepo<Order>
    {
        Task<Order> GetOrderById (Guid id);
        Task<List<Order>> GetAllOrderAsync(params Expression<Func<Order, object>>[] includes);
    }
}
