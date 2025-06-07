using Application.Domain;
using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MemberService(IUnitOfWork uow, IMapper mapper)
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
            }catch (Exception ex)
            {
                return rp.SetBadRequest(ex.Message);
            }
        }
    }
}
