using Application.IRepos;
using Domain;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class EmployeeRepo : GenericRepo<Employee>, IEmployeeRepo
    {
        private readonly AppDbContext _appDbContext;

        public EmployeeRepo(AppDbContext dbContext) : base(dbContext)
        {
            _appDbContext = dbContext;
        }
    }
}
