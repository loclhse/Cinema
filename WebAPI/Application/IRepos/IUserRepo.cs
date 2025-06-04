using Domain.Entities;

namespace Application.IRepos
{
    public interface IUserRepo : IGenericRepo<AppUser>
    {
        Task<AppUser?> GetEmployeeAccount(Guid id);
        Task<IEnumerable<AppUser>> GetAllEmployeeAccounts();
        Task<bool> IsEmailExists(string email);
    }
}
