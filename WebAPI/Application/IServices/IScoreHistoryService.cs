using Application.ViewModel;
using Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IScoreHistoryService
    {
        Task<ApiResp> ViewHistory(Guid userId);
        Task<ApiResp> DeleteHistory(Guid userId, Guid ScoreLogId);
    }
}
