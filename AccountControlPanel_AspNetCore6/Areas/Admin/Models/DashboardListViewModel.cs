using Entities.Dtos;

namespace AccountControlPanel_AspNetCore6.Areas.Admin.Models
{
    public class DashboardListViewModel
    {
        public IEnumerable<UserDto> Users
        {
        get; set; } = Enumerable.Empty<UserDto>();
        public Pagination? Pagination
        {
            get; set;
        } = new();
        public int TotalCount => Users.Count();
    }
}
