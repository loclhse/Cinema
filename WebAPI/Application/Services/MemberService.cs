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
using System.Text;
using System.Threading.Tasks;
using static Application.IServices.IUserService;

namespace Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAuthRepo _authRepo;
        public MemberService(IUnitOfWork uow, IMapper mapper, IAuthRepo authRepo)
        {
            _uow = uow;
            _mapper = mapper;
            _authRepo = authRepo;
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

        public async Task<ApiResp> GetAllMember()
        {
            ApiResp rp = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member);
                var prof = await _uow.UserRepo.GetAllMemberAccountsAsync();

                var dto = _mapper.Map<List<CustomerResponse>>(BuildJoin(ids, prof));
                if (!dto.Any())
                {
                    return rp.SetNotFound("Not found any Member");
                }
                return rp.SetOk(dto);
            }
            catch (Exception ex)
            {
                return rp.SetBadRequest(ex.Message);
            }
        }
        public async Task<ApiResp> SearchMembers(string value, SearchKey searchKey)
        {
            var resp = new ApiResp();
            try
            {
                var ids = await _uow.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member);
                var members = await _uow.UserRepo.GetAllMemberAccountsAsync();
                IEnumerable<AppUser> result;
                switch (searchKey)
                {
                    case SearchKey.IdentityCard:
                        result = members.Where(c => c.IdentityCard != null && c.IdentityCard.Contains(value));
                        break;
                    case SearchKey.PhoneNumber:
                        result = members.Where(c => c.Phone != null && c.Phone.Contains(value));
                        break;
                    case SearchKey.Name:
                        result = members.Where(c => c.FullName != null && c.FullName.Contains(value));
                        break;
                    default:
                        return resp.SetBadRequest("Invalid search key.");
                }
                if (!result.Any())
                    return resp.SetNotFound("No members found.");
                var responses = _mapper.Map<List<CustomerResponse>>(BuildJoin(ids, result));
                return resp.SetOk(responses);
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResp> DeleteMemberAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var profile = await _uow.UserRepo.GetMemberAccountAsync(id);
                if (profile == null) return resp.SetNotFound("Mebers not found.");

                profile.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return resp.SetOk("Member deleted.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
        public async Task<ApiResp> UpdateMemberAsync(Guid id, CustomerUpdateResquest req)
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
        public async Task<ApiResp> CustomerToMember(Guid CusId)
        {
            var resp = new ApiResp();
            try
            {
                var customer = await _uow.UserRepo.GetCustomerAccountAsync(CusId);
                if (customer == null) return resp.SetNotFound("Customer not found.");
                await _authRepo.RemoveUserFromRoleAsync(customer.Id, "Customer");
                await _authRepo.AddUserToRoleAsync(customer.Id, "Member");
                await _uow.SaveChangesAsync();
                return resp.SetOk("Customer converted to Member successfully.");
            }
            catch (Exception ex) { return resp.SetBadRequest(ex.Message); }
        }
    } 
}
