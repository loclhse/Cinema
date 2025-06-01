using Domain.Entities;

namespace Application.IRepos
{
    public interface IUserRepo : IGenericRepo<User>
    {
        Task<User?> GetEmployeeAccount(int id);
        Task<IEnumerable<User>> GetAllEmployeeAccounts();
        Task<bool> IsEmailExists(string email);
    }
}
