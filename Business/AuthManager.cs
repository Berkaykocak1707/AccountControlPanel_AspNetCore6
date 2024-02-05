using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Business.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Business
{
    public class AuthManager : IAuthService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly PhotoInsertService _photoInsertService;
        public AuthManager(RoleManager<IdentityRole> roleManager,
        UserManager<CustomUser> userManager,
        IMapper mapper,
        PhotoInsertService photoInsertService,
        SignInManager<CustomUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _photoInsertService = photoInsertService;
            _signInManager = signInManager;
        }

        public IEnumerable<IdentityRole> Roles =>
            _roleManager.Roles;

        public IEnumerable<CustomUser> Users => _userManager.Users.ToList();

        public async Task<IdentityResult> CreateUserAsync(UserDtoForCreation userDto,IFormFile? photo)
        {
            var user = _mapper.Map<CustomUser>(userDto);
            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (result.Succeeded)
            {
                var createdUser = await _userManager.FindByNameAsync(user.UserName);
                await _userManager.AddToRoleAsync(createdUser, "User");
                if (photo != null)
                {
                    var photoPath = await _photoInsertService.PhotoInsertAsync(photo, createdUser.UserName, createdUser.Id);
                    createdUser.UserPhotoUrl = photoPath;
                    await _userManager.UpdateAsync(createdUser);
                }
            }
            if (!result.Succeeded)
            {
                return result;
            }
            if (userDto.Roles.Count > 0)
            {
                var roleResult = await _userManager.AddToRolesAsync(user, userDto.Roles);
                if (!roleResult.Succeeded)
                    throw new Exception("System have problems with roles.");
            }

            return result;
        }
        public async Task<IdentityResult> DeleteOneUserAsync(string userName)
        {
            if (userName != "Admin")
            {
                var user = await GetOneUserAsync(userName);
                return await _userManager.DeleteAsync(user);
            }
            throw new Exception("You can't delete Admin");
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber,int pageSize)
        {
            var users = _userManager.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    LockoutEnabled = user.LockoutEnabled,
                    FirstName = user.FirstName, // Bu alanlar IdentityUser modelinizde varsa
                    LastName = user.LastName,   // Bu alanlar IdentityUser modelinizde varsa
                    UserPhotoUrl = user.UserPhotoUrl, // Bu alanlar IdentityUser modelinizde varsa
                    DateOfBirth = user.DateOfBirth,   // Bu alanlar IdentityUser modelinizde varsa
                    Roles = new HashSet<string>(roles)
                };

                userDtos.Add(userDto);
            }
            return userDtos;
        }

        public async Task<CustomUser> GetOneUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
                return user;
            return null;
        }
        public async Task<UserDto> GetOneUserForHomeAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
                return _mapper.Map<UserDto>(user);
            return new UserDto { UserName = userName };
        }
        public async Task<UserDtoForUpdate> GetOneUserForUpdateAsync(string userName)
        {
            var user = await GetOneUserAsync(userName);
            var userDto = _mapper.Map<UserDtoForUpdate>(user);
            userDto.Roles = new HashSet<string>(Roles.Select(r => r.Name).ToList());
            userDto.UserRoles = new HashSet<string>(await _userManager.GetRolesAsync(user));
            return userDto;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await GetOneUserAsync(model.UserName);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (isPasswordValid)
            {
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, model.Password);
                return result;
            }
            throw new Exception("Old password is wrong"); //Check old password in Controller!
        }
        public async Task UpdateForUserAsync(UserDto userDto,string userName, IFormFile? file, bool deletePhoto)
        {
            var user = await GetOneUserAsync(userName);
            if (file is not null)
            {
                var path = await _photoInsertService.PhotoInsertAsync(file, userDto.UserName, user.Id);
                userDto.UserPhotoUrl = path;
            }
            else
            {
                userDto.UserPhotoUrl = user.UserPhotoUrl;
            }
            if (deletePhoto == true && user.UserPhotoUrl != null)
            {
                userDto.UserPhotoUrl = null;
            }
            _mapper.Map(userDto, user);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
        }
        public async Task UpdateAsync(UserDtoForUpdate userDto)
        {
            var user = await GetOneUserAsync(userDto.UserName);
            if (userDto.Roles.Count > 0)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var r1 = await _userManager.RemoveFromRolesAsync(user, userRoles);
                var r2 = await _userManager.AddToRolesAsync(user, userDto.Roles);
            }
        }

        public async Task<IdentityResult> ChangesUserStatusAsync(string userName)
        {
            if (userName == "Admin")
            {
                throw new Exception("You can't change status this User");
            }
            var user = await GetOneUserAsync(userName);
            if (user != null)
            {
                if (user.LockoutEnabled == true)
                {
                    user.LockoutEnabled = false;
                }
                else if (user.LockoutEnabled == false)
                {
                    user.LockoutEnabled = true; 
                }
                var result =  await _userManager.UpdateAsync(user);
                return result;
            }
            throw new Exception("User is null");
        }

        public async Task<IdentityResult> AddUserLoginAsync(CustomUser user,UserLoginInfo loginInfo)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);
            return await _userManager.AddLoginAsync(user,loginInfo);
        }

        public async Task<string> CheckUserLoginAsync(string loginProvider, string providerKey,UserDtoForCreation? userDtoFor)
        {
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                return "Ok.";
            }
            else if (userDtoFor is not null)
            {
                var email = await _userManager.FindByEmailAsync(userDtoFor.Email);
                if (email == null)
                {
                    var newUsername = userDtoFor.UserName + "Twitter";
                    userDtoFor.UserName = newUsername;
                    await CreateUserAsync(userDtoFor, null);
                    var userNew = await _userManager.FindByNameAsync(newUsername);
                    var loginInfo = new UserLoginInfo("Twitter", providerKey, "Twitter");
                    await AddUserLoginAsync(userNew, loginInfo);
                    await _signInManager.SignInAsync(userNew, isPersistent: false);
                        
                    return "Ok.";
                }
                return "This email is in use. If you have forgotten your password, you can reset your password with your username.";
            }
            return "Bad.";
        }
    }
}