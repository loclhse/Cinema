using Application.DTOs.DtoRequest;
using Infrastructure.Entities;

namespace Application.IRepos
{
    public interface IUserRepo 
    {
        Task<User> FindByIdAsync(Guid id);
        void Update(User user);
    }
}
