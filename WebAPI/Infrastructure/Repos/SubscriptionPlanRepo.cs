using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class SubscriptionPlanRepo : GenericRepo<SubscriptionPlan>, ISubscriptionPlanRepo
    {
        public SubscriptionPlanRepo(AppDbContext context) : base(context)
        {
        }
    }
}
