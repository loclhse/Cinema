using Application.DTOs.DtoRequest;
using Application.DTOs.DtoResponse;
using Application.IServices;
using AutoMapper;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper  _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<UserDto> FindByIdAsync(Guid id)
        {
            var user = await _unitOfWork.UserRepo.FindByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserUpdateResponse> UpdateUserAsync(Guid id, UserUpdateRequest userDto)
        {
            var user = await _unitOfWork.UserRepo.FindByIdAsync(id);
            _mapper.Map(userDto, user);

            _unitOfWork.UserRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserUpdateResponse>(user);
        }
    }
}
