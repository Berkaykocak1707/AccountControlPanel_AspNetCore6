using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Business.Contracts
{
    public interface IAuthService
    {
        IEnumerable<IdentityRole> Roles
        {
            get;
        }
        IEnumerable<CustomUser> Users
        {
            get;
        }
        Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<CustomUser> GetOneUserAsync(string userName);
        Task<UserDtoForUpdate> GetOneUserForUpdateAsync(string userName);
        Task<UserDto> GetOneUserForHomeAsync(string userName);
        Task UpdateForUserAsync(UserDto userDto,string userName, IFormFile? file, bool deletePhoto);
        Task<IdentityResult> CreateUserAsync(UserDtoForCreation userDto, IFormFile? file);
        Task UpdateAsync(UserDtoForUpdate userDto);
        Task<IdentityResult> AddUserLoginAsync(CustomUser user, UserLoginInfo loginInfo);
        Task<string> CheckUserLoginAsync(string loginProvider, string providerKey, UserDtoForCreation? userDtoFor);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
        Task<IdentityResult> DeleteOneUserAsync(string userName);
        Task<IdentityResult> ChangesUserStatusAsync(string userName);
    }
}
