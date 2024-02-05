using AccountControlPanel_AspNetCore6.Areas.Admin.Models;
using Business.Contracts;
using Entities.Dtos;
using Entities.RequestParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AccountControlPanel_AspNetCore6.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IServiceManager _services;

        public DashboardController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index([FromQuery] UserRequestParameter userRequest)
        {
            var Users = await _services.AuthService.GetAllUsersAsync(userRequest.PageNumber, userRequest.PageSize);
            var userCount = _services.AuthService.Users.Count();
            var Pagination = new Pagination()
            {
                TotalItems = userCount,
                CurrentPage = userRequest.PageNumber,
                ItemsPerPage = userRequest.PageSize
            };
            TempData["TotalCount"] = userCount;
            string Show = (userRequest.PageNumber * userRequest.PageSize).ToString();
            TempData["TotalShow"] = Show;
            return View(new DashboardListViewModel() 
            {
                Pagination = Pagination,
                Users = Users
            });
        }
        public async Task<IActionResult> DeleteUser([FromRoute]string id)
        {
            if (id != null || id != "Admin")
            {
                await _services.AuthService.ChangesUserStatusAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RoleChange(string userName,bool Admin,bool User, bool Editor)
        {
            var user = await _services.AuthService.GetOneUserForUpdateAsync(userName);
            HashSet<string> roles = new HashSet<string>();
            if (Admin == true)
            {
                roles.Add("Admin");
            }
            if (User == true)
            {
                roles.Add("User");
            }
            if (Editor == true)
            {
                roles.Add("Editor");
            }
            var model = new UserDtoForUpdate()
            {
                UserName = userName,
                Roles = roles
            };
            await _services.AuthService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }
    }
}
