using Application.DTOs.DtoRequest;
using Application.IRepos;
using Infrastructure.Entities;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<User>, IUserRepo
    {
        private readonly AppDbContext _appDBContext;

        public UserRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<User> FindByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public void Update(User user)
        {
            base.Update(user);
        }
    }
}
    
