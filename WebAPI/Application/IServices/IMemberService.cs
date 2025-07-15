using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.IServices.IUserService;

namespace Application.IServices
{
    public interface IMemberService
    {
        Task<ApiResp> GetAllMember();
        Task<ApiResp> SearchMembers(string value, SearchKey searchKey);
        Task<ApiResp> DeleteMemberAsync(Guid id);
        Task<ApiResp> UpdateMemberAsync(Guid id, CustomerUpdateResquest req);
    }
}
