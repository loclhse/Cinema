using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<AppUser>, IUserRepo
    {
        private readonly AppDbContext _appDBContext;

        public UserRepo(AppDbContext context) : base(context)
        {
            _appDBContext = context;
        }

    }
}
