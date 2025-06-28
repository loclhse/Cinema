using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class PaymentRepo : GenericRepo<Payment>, IPaymentRepo
    {
        public PaymentRepo(AppDbContext context) : base(context)
        {
        }
        
      
    }
}
