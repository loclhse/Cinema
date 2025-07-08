using Application.IRepos;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
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
    public class OrderRepo : GenericRepo<Order>, IOrderRepo
    {
        private AppDbContext _appDbContext;
        public OrderRepo(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<Order> GetOrderById (Guid guid)
        {
            var order = await _appDbContext.Orders
                .Include(o => o.SeatSchedules)
                .Include(o => o.SnackOrders)// if you want to include related entities
                .FirstOrDefaultAsync(o => o.Id == guid);

            if (order == null)
                return null;

            return order;
        }


    }
}
