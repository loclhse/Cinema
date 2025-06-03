using Application.IRepos;
using Domain.Entities;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<AppUser>, IUserRepo
    {
        private readonly AppDbContext _appDBContext;

        public UserRepo(AppDbContext context) : base(context)
        {
            _appDBContext = context;
        }
        // Add any specific methods for UserRepo here if needed

    }
}
