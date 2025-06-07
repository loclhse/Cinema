using Application.Domain;
using Application.IRepos;
using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;
using static Application.IServices.IUserService;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                                   .GetIdentityUsersByRoleAsync(RoleNames.Customer))
                                   .FirstOrDefault(i => i.Id == id);
                var profile = await _uow.UserRepo.GetCustomerAccountAsync(id);

                if (idUser == null || profile == null)
                    return resp.SetNotFound("Customer not found.");

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
                var profile = await _uow.UserRepo.GetCustomerAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Customer not found.");

                profile.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return resp.SetOk("Customer deleted.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> UpdateCustomerAsync(Guid id, CustomerUpdateResquest req)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetCustomerAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Customer not found.");

                _mapper.Map(req, profile);
                await _uow.SaveChangesAsync();
                return resp.SetOk("Customer updated.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
        public async Task<ApiResp> SearchCustomers(string value, SearchKey searchKey)
        {
            var resp = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer);
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
                var responses = _mapper.Map<List<CustomerResponse>>(BuildJoin(ids,result));
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
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee);
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
                var responses = _mapper.Map<List<EmployeeResponse>>(BuildJoin(ids, result));
                if (!result.Any()) 
                { 
                    return resp.SetNotFound("No employees found."); 
                }
                return resp.SetOk(responses);
                     
                

            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> GetDeletedAccountsAsync()
        {
            var apiresponse = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee);
                var list = await _uow.UserRepo.GetAllEmployeeAccountsDeletedAsync();
                var rs = _mapper.Map<List<EmployeeResponse>>(BuildJoin(ids, list));
                return apiresponse.SetOk(rs);

            }catch(Exception ex)
            {
                return apiresponse.SetNotFound(ex.Message);
            }
        }

        public async Task<ApiResp> RestoreAccountAsync(Guid id)
        {
            var apiresponse = new ApiResp();
            try
            {
                var idUser = (await _uow.UserRepo
                                 .GetIdentityUsersByRoleAsync(RoleNames.Employee))
                                 .FirstOrDefault(i => i.Id == id);
                var dlEmp = _uow.UserRepo.GetDeletedEmployeeAccountAsync(id);

                if (idUser == null || dlEmp == null)
                {
                    return apiresponse.SetNotFound("Member not found.");
                }
                 var account = await _uow.UserRepo.GetAsync(a => a.Id == idUser.Id); 
                  account.IsDeleted = false;
                  await _uow.SaveChangesAsync();
                return apiresponse.SetOk("Restore Successfully!!!!");

            }
            catch (Exception ex)
            {
                return apiresponse.SetBadRequest(ex.Message);
            }
      }
        public async Task<ApiResp> SearchIsDeleteEmployees(string value, SearchKey searchKey)
        {
            var resp = new ApiResp();
            try
            {
                var employees = await _uow.UserRepo.GetAllEmployeeAccountsDeletedAsync();
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee);
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
                if (!result.Any())
                    return resp.SetNotFound("No employees found.");
                var responses = _mapper.Map<List<EmployeeResponse>>(BuildJoin(ids,result));
                return resp.SetOk(responses);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
     }
}

