using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class NormalizeStringService
    {
        public string NormalizeString(string input)
        {
            input = input.Replace("ğ", "g").Replace("Ğ", "G")
                         .Replace("ü", "u").Replace("Ü", "U")
                         .Replace("ş", "s").Replace("Ş", "S")
                         .Replace("ı", "i").Replace("İ", "I")
                         .Replace("ö", "o").Replace("Ö", "O")
                         .Replace("ç", "c").Replace("Ç", "C");

            input = input.Replace(" ", "");

            return input;
        }
    }
}
