using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
using System.Data.Entity;
namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<User>, IUserRepo
    {
        private readonly AppDbContext _appDBContext;

        public UserRepo(AppDbContext context) : base(context)
        {
            _appDBContext = context;
        }

        public async Task<IEnumerable<User>> GetAllEmployeeAccounts()
        {
            var accounts = _appDBContext.Users;
            var employees = new List<User>();
            for (int i = 0; i < accounts.Count(); i++)
            {
                if (accounts.ElementAt(i).role == Role.Employee)
                {
                    employees.Add(accounts.ElementAt(i));
                }
            }
            return employees.AsEnumerable();
        }

        public Task<User?> GetEmployeeAccount(int id)
        {
            var employee = _appDBContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.role == Role.Employee);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            return employee;
        }

        public async Task<bool> IsEmailExists(string email)
        {
            return await _appDBContext.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }
    }
}
