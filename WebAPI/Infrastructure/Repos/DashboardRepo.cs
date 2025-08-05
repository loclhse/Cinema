using Application.IRepos;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class DashboardRepo : GenericRepo<object>, IDashboardRepo
    {
        public DashboardRepo(AppDbContext context) : base(context)
        {
        }
    }
} 