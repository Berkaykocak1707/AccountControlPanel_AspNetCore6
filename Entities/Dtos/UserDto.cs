using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos
{
    public record UserDto
    {
        [DataType(DataType.Text)]
        [Required(ErrorMessage ="UserName is required.")]
        public string? UserName { get; set; }
        
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage ="Email is required.")]
        public string? Email { get; init; }
        
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; init; }
        public string? FirstName
        {
            get; init;
        }
        public string? LastName
        {
            get; init;
        }
        public string? UserPhotoUrl
        {
            get; set;
        }
        [DataType(DataType.DateTime)]
        public DateTime? DateOfBirth
        {
            get; init;
        }
        public bool? LockoutEnabled
        {
            get; init;
        }

        public HashSet<string>? Roles { get; set; } = new HashSet<string>();
    }
}