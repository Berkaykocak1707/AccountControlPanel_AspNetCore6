using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class PhotoInsertService
    {
        public async Task<string> PhotoInsertAsync(IFormFile file, string userName, string userId)
        {
            Random rnd = new Random();
            int randomNumber = rnd.Next(0, 101);
            string originalFileName = file.FileName;
            string fileExtension = Path.GetExtension(originalFileName);
            string newFileName = userName + "-" + randomNumber + fileExtension;

            string userDirectoryPath = Path.Combine("images", userId);
            string physicalUserDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userDirectoryPath);

            // Klasör yoksa oluştur
            if (!Directory.Exists(physicalUserDirectoryPath))
            {
                Directory.CreateDirectory(physicalUserDirectoryPath);
            }

            string physicalPath = Path.Combine(physicalUserDirectoryPath, newFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string virtualPath = Path.Combine("/", userDirectoryPath, newFileName); 
            return virtualPath.Replace("\\", "/");
        }
    }
}
