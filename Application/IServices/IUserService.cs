using Application.DTOs.DtoRequest;
using Application.DTOs.DtoResponse;

namespace Application.IServices
{
    public interface IUserService
    {
        Task<UserDto> FindByIdAsync(Guid id);
        Task<UserUpdateResponse> UpdateUserAsync(Guid id, UserUpdateRequest userDto);

    }
}
