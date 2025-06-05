using Application.Domain;
using Application.IRepos;
using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Application.IServices.IUserService;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        private static IEnumerable<IdentityWithProfile> BuildJoin(
            IEnumerable<DomainUser> ids,
            IEnumerable<AppUser> profs)
        {
            return profs.Join(ids,
                       p => p.Id,
                       i => i.Id,
                       (p, i) => new IdentityWithProfile { Identity = i, Profile = p })
                        .ToList();
        }

        /*  EMPLOYEE */

        public async Task<ApiResp> GetAllEmployeesAsync()
        {
            var resp = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee);
                var prof = await _uow.UserRepo.GetAllEmployeeAccountsAsync();

                var dto = _mapper.Map<List<EmployeeResponse>>(BuildJoin(ids, prof));
                return resp.SetOk(dto);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> GetEmployeeByIdAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var idUser = (await _uow.UserRepo
                                   .GetIdentityUsersByRoleAsync(RoleNames.Employee))
                                   .FirstOrDefault(i => i.Id == id);
                var profile = await _uow.UserRepo.GetEmployeeAccountAsync(id);

                if (idUser == null || profile == null)
                    return resp.SetNotFound("Employee not found.");

                var dto = _mapper.Map<EmployeeResponse>(
                              new IdentityWithProfile { Identity = idUser, Profile = profile });

                return resp.SetOk(dto);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> DeleteEmployeeAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetEmployeeAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Employee not found.");

                profile.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return resp.SetOk("Employee deleted.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> UpdateEmployeeAsync(Guid id, EmployeeUpdateResquest req)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetEmployeeAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Employee not found.");

                _mapper.Map(req, profile);
                await _uow.SaveChangesAsync();
                return resp.SetOk("Employee updated.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }


        /* MEMBER */

        public async Task<ApiResp> GetAllCustomersAsync()
        {
            var resp = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer);
                var prof = await _uow.UserRepo.GetAllCustomerAccountsAsync();

                var dto = _mapper.Map<List<CustomerResponse>>(BuildJoin(ids, prof));
                return resp.SetOk(dto);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> GetCustomerByIdAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var idUser = (await _uow.UserRepo
                                   .GetIdentityUsersByRoleAsync(RoleNames.Member))
                                   .FirstOrDefault(i => i.Id == id);
                var profile = await _uow.UserRepo.GetMemberAccountAsync(id);

                if (idUser == null || profile == null)
                    return resp.SetNotFound("Member not found.");

                var dto = _mapper.Map<CustomerResponse>(
                              new IdentityWithProfile { Identity = idUser, Profile = profile });

                return resp.SetOk(dto);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> DeleteCustomerAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetMemberAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Member not found.");

                profile.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return resp.SetOk("Member deleted.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> UpdateCustomerAsync(Guid id, MemberUpdateResquest req)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetMemberAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Member not found.");

                _mapper.Map(req, profile);
                await _uow.SaveChangesAsync();
                return resp.SetOk("Member updated.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
        public async Task<ApiResp> SearchCustomers(string value, SearchKey searchKey)
        {
            var resp = new ApiResp();
            try
            {
                var customers = await _uow.UserRepo.GetAllCustomerAccountsAsync();
                IEnumerable<AppUser> result;
                switch (searchKey)
                {
                    case SearchKey.IdentityCard:
                        result = customers.Where(c => c.IdentityCard.Contains(value));
                        break;
                    case SearchKey.PhoneNumeber:
                        result = customers.Where(c => c.Phone.Contains(value));
                        break;
                    case SearchKey.Name:
                        result = customers.Where(c => c.FullName.Contains(value));
                        break;
                    default:
                        return resp.SetBadRequest("Invalid search key.");
                }
                if (!result.Any())
                    return resp.SetNotFound("No customers found.");
                var responses = _mapper.Map<List<CustomerResponse>>(result);
                return resp.SetOk(responses);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
        public async Task<ApiResp> SearchEmployees(string value, SearchKey searchKey)
        {
            var resp = new ApiResp();
            try
            {
                var employees = await _uow.UserRepo.GetAllEmployeeAccountsAsync();
                IEnumerable<AppUser> result;
                switch (searchKey)
                {
                    case SearchKey.IdentityCard:
                        result = employees.Where(c => c.IdentityCard.Contains(value));
                        break;
                    case SearchKey.PhoneNumeber:
                        result = employees.Where(c => c.Phone.Contains(value));
                        break;
                    case SearchKey.Name:
                        result = employees.Where(c => c.FullName.Contains(value));
                        break;
                    default:
                        return resp.SetBadRequest("Invalid search key.");
                
                }
                var responses = _mapper.Map<List<EmployeeResponse>>(result);
                if (!result.Any()) 
                { 
                    return resp.SetNotFound("No employees found."); 
                }
                return resp.SetOk(responses);
                     
                

            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
    }
}
