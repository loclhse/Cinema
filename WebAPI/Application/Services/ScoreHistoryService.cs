using Application.IServices;
using Application.ViewModel;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ScoreHistoryService : IScoreHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ScoreHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResp> DeleteHistory(Guid userId, Guid ScoreLogId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var scoreLog = await _unitOfWork.ScoreLogRepo.GetAsync(x => x.Id == ScoreLogId && x.IsDeleted == false);
                if (scoreLog == null)
                {
                    return apiResp.SetNotFound("Not found");
                }
                scoreLog.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Deleted");
            }
            catch
            {
                return apiResp.SetBadRequest();
            }
        }

        public async Task<ApiResp> ViewHistory(Guid userId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                List<ScoreLog> scoreLogs = await _unitOfWork.ScoreLogRepo.GetAllAsync(x => x.UserId == userId && x.IsDeleted == false);
                if(!scoreLogs.Any())
                {
                    return apiResp.SetNotFound("Not found");
                }
                return apiResp.SetOk(scoreLogs);
            }
            catch
            {
                return apiResp.SetBadRequest();
            }
        }
    }
}
