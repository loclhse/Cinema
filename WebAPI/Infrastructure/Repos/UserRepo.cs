using Application.IRepos;
using Infrastructure.Entities;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<User>, IUserRepo
    {
        private readonly AppDbContext _appDBContext;

        public UserRepo(AppDbContext context) : base(context)
        {
            _appDBContext = context;
        }
        // Add any specific methods for UserRepo here if needed
    }
}
