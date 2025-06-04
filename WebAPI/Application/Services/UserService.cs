using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.ViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata;
using static Application.IServices.IUserService;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }




        public async Task<ApiResp> DeleteAccountAsync(Guid id)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var account = await _unitOfWork.UserRepo.GetAsync(e => e.IsDeleted == false && e.Id == id);
                if (account == null)
                {
                    return apiResp.SetNotFound("Account does not exist!!");
                }
                account.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Delete Successfully");

            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }



        public async Task<ApiResp> GetAllEmployeesAsync()
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var employees = await _unitOfWork.UserRepo.GetAllAsync(e => e.IsDeleted == false);
                var employeesReal = new List<AppUser>();
                foreach (var employee in employees)
                {
                    var check = await _unitOfWork.UserRepo.IsEmployeeAccount(employee.Id);
                    if (check == 1)
                    {
                        employeesReal.Add(employee);
                    }
                }
                if(!employeesReal.Any())
                {
                    return apiResp.SetNotFound("No employee found");
                }
                var response = _mapper.Map<List<EmployeeResponse>>(employeesReal);
                return apiResp.SetOk(response);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetAllMembesAsync()
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var Cus = await _unitOfWork.UserRepo.GetAllAsync(e => e.IsDeleted == false);
                var CusReal = new List<AppUser>();
                foreach (var cus in Cus)
                {
                    var check = await _unitOfWork.UserRepo.IsCustomerAccount(cus.Id);
                    if (check == 1)
                    {
                        CusReal.Add(cus);
                    }
                }
                if(!CusReal.Any())
                {
                    return apiResp.SetNotFound("No customer found.");
                }
                var response = _mapper.Map<List<MemberResponse>>(CusReal);
                return apiResp.SetOk(response);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetEmployeeByIdAsync(Guid id)
        {
            var apiResp = new ApiResp();
            try
            { 
                var employee = await _unitOfWork.UserRepo.GetAsync(c => c.Id == id && c.role == Domain.Enums.Role.Employee && c.IsDeleted == false);
                if (employee == null)
                {
                    return apiResp.SetNotFound("Can not find the employee's detail");
                }
                var response = _mapper.Map<EmployeeResponse>(employee);
                return apiResp.SetOk(response);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetMemberByIdAsync(Guid id)
        {
            var apiResp = new ApiResp();
            try
            {
                var member = await _unitOfWork.UserRepo.GetAsync(c => c.Id == id && c.role == Domain.Enums.Role.Member && c.IsDeleted == false);
                if (member == null)
                {
                    return apiResp.SetNotFound("Can not find the member's detail");
                }
                var response = _mapper.Map<MemberResponse>(member);
                return apiResp.SetOk(response);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> SearchEmployeeAsync(string searchValue, SearchKey searchKey)
        {
           ApiResp apiResp = new ApiResp();
            try
            {
                if(searchKey == SearchKey.Identitycart)
                {
                    var employees = await _unitOfWork.UserRepo.GetAllAsync(e => e.Identitycart.Contains(searchValue) && e.role == Domain.Enums.Role.Employee && e.IsDeleted == false);
                    if (employees == null || !employees.Any())
                    {
                        return apiResp.SetNotFound("No employee found with the provided Identitycart.");
                    }
                    var response = _mapper.Map<List<EmployeeResponse>>(employees);
                    return apiResp.SetOk(response);
                }
                else if (searchKey == SearchKey.PhoneNumeber)
                {
                    var employees = await _unitOfWork.UserRepo.GetAllAsync(e => e.Phone.Contains(searchValue) && e.role == Domain.Enums.Role.Employee && e.IsDeleted == false);
                    if (employees == null || !employees.Any())
                    {
                        return apiResp.SetNotFound("No employee found with the provided Phone Number.");
                    }
                    var response = _mapper.Map<List<EmployeeResponse>>(employees);
                    return apiResp.SetOk(response);
                }
                else if (searchKey == SearchKey.Name)
                {
                    var employees = await _unitOfWork.UserRepo.GetAllAsync(e => e.FullName.Contains(searchValue) && e.role == Domain.Enums.Role.Employee && e.IsDeleted == false);
                    if (employees == null || !employees.Any())
                    {
                        return apiResp.SetNotFound("No employee found with the provided Name.");
                    }
                    var response = _mapper.Map <List<EmployeeResponse>> (employees);
                    return apiResp.SetOk(response);
                }
                else
                {
                    return apiResp.SetBadRequest("Invalid search key provided.");
                }
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message) ;
            }
        }

        public async Task<ApiResp> SearchMemberAsync(string searchValue, SearchKey searchKey)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                //Search by Identitycart
                if (searchKey == SearchKey.Identitycart)
                {
                    var members = await _unitOfWork.UserRepo.GetAllAsync(e => e.Identitycart.Contains(searchValue) && e.role == Domain.Enums.Role.Member && e.IsDeleted == false);
                    if (members == null || !members.Any())
                    {
                        return apiResp.SetNotFound("No member found with the provided Identitycart.");
                    }
                    var response = _mapper.Map<List<MemberResponse>> (members);
                    return apiResp.SetOk(response);
                }
                //Search by Phone Number
                else if (searchKey == SearchKey.PhoneNumeber)
                {
                    var members = await _unitOfWork.UserRepo.GetAllAsync(e => e.Phone.Contains(searchValue) && e.role == Domain.Enums.Role.Member && e.IsDeleted == false);
                    if (members == null || !members.Any())
                    {
                        return apiResp.SetNotFound("No member found with the provided Phone Number.");
                    }
                    var response = _mapper.Map<List<MemberResponse>>(members);
                    return apiResp.SetOk(response);
                }
                //Search by Name
                else if (searchKey == SearchKey.Name)
                {
                    var members = await _unitOfWork.UserRepo.GetAllAsync(e => e.FullName.Contains(searchValue) && e.role == Domain.Enums.Role.Member && e.IsDeleted == false);
                    if (members == null || !members.Any())
                    {
                        return apiResp.SetNotFound("No member found with the provided Name.");
                    }
                    var response = _mapper.Map<List<MemberResponse>>(members);
                    return apiResp.SetOk(response);
                }
                else
                {
                    return apiResp.SetBadRequest("Invalid search key provided.");
                }
            }
            catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateEmployeeAsync(Guid id, EmployeeUpdateResquest employeeUpdateResquest)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var employee = await _unitOfWork.UserRepo.GetAsync(c => c.Id == id);
                if (employee == null)
                {
                    return apiResp.SetNotFound("Can not find the employee's  detail");
                }
                _mapper.Map(employeeUpdateResquest, employee);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk(" employee's details updated successfully");

            }
            catch (Exception e)
            {
                return apiResp.SetBadRequest(e.Message);
            }
        }

        public async Task<ApiResp> UpdateMemberAsync(Guid id, MemberUpdateResquest memberUpdateResquest)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var member = await _unitOfWork.UserRepo.GetAsync(c => c.Id == id);
                if (member == null)
                {
                    return apiResp.SetNotFound("Can not find the member's detail");
                }
                _mapper.Map(memberUpdateResquest, member);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Member's details updated successfully");

            }
            catch (Exception e)
            {
                return apiResp.SetBadRequest(e.Message);
            }
        }
    }
       
}
