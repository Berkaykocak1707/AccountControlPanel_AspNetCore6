using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class CustomUser : IdentityUser
    {
        public string? FirstName
        {
            get; set;
        }
        public string? LastName
        {
            get; set;
        }
        public string? UserPhotoUrl
        {
            get; set; 
        }
        public DateTime? DateOfBirth
        {
            get; set; 
        }

    }
}
