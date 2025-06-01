using Application.IServices;
using Application.ViewModel;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<ReadEmployeeAccount?> GetEmployeeAccountByIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepo.GetEmployeeAccount(id);
            return _mapper.Map<ReadEmployeeAccount>(user);
        }

        public async Task<User> CreateEmployeeAccountAsync(WriteEmloyeeAccount employeeAccount)
        {
            if (string.IsNullOrWhiteSpace(employeeAccount.Password) ||
                string.IsNullOrEmpty(employeeAccount.Email))
                {
                throw new Exceptions.ApplicationException( HttpStatusCode.BadRequest,"Password and Email are required fields for creating an employee account.");
            }
            if (await _unitOfWork.UserRepo.IsEmailExists(employeeAccount.Email))
            {
                throw new Exceptions.ApplicationException(HttpStatusCode.Conflict, "An account with this email already exists.");
            }
            try
            {
                var user = _mapper.Map<User>(employeeAccount);
                user.Password = BCrypt.Net.BCrypt.HashPassword(employeeAccount.Password, workFactor: 12);
                user.Username = employeeAccount.Email; // Assuming username is the same as email
                user.role = Domain.Enums.Role.Employee;

                await _unitOfWork.UserRepo.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an employee account.");
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAccountAsync(int id)
        {
            var user = await _unitOfWork.UserRepo.GetEmployeeAccount(id);
            if (user == null)
            {
                _logger.LogWarning("Attempted to delete a non-existing employee account with ID {Id}.", id);
                return false;
            }
            try
            {
                _unitOfWork.UserRepo.SoftDelete(user);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the employee account with ID {Id}.", id);
                throw;
            }
        }

        public async Task<List<ReadEmployeeAccount>> GetAllEmployeeAccountsAsync()
        {
            return await _unitOfWork.UserRepo.GetAllEmployeeAccounts()
                .ContinueWith(task => task.Result.Select(user => _mapper.Map<ReadEmployeeAccount>(user)).ToList());
        }

        public async Task<User?> UpdateEmployeeAccountAsync(int id, WriteEmloyeeAccount employeeAccount)
        {
           var user = _unitOfWork.UserRepo.GetEmployeeAccount(id);
            if (user == null)
            {
                _logger.LogWarning("Attempted to update a non-existing employee account with ID {Id}.", id);
                return await Task.FromResult<User?>(null);
            }
            try
            {
                user.Result.FullName = employeeAccount.FullName;
                user.Result.Email = employeeAccount.Email;
                user.Result.Phone = employeeAccount.Phone;
                user.Result.Address = employeeAccount.Address;
                user.Result.Identitycart = employeeAccount.Identitycart;
                user.Result.Dob = employeeAccount.Dob;
                user.Result.Sex = employeeAccount.Sex;

                if (!string.IsNullOrWhiteSpace(employeeAccount.Password))
                {
                    user.Result.Password = BCrypt.Net.BCrypt.HashPassword(employeeAccount.Password, workFactor: 12);
                }

                _unitOfWork.UserRepo.Update(user.Result);
                _unitOfWork.SaveChangesAsync();

                return await Task.FromResult(user.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the employee account with ID {Id}.", id);
                throw;
            }
        }
    }
}
