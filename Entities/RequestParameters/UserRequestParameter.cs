using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.RequestParameters
{
    public class UserRequestParameter
    {
        public int PageNumber
        {
            get; set;
        }
        public int PageSize
        {
            get; set;
        }
        public UserRequestParameter() : this(1, 5)
        {

        }
        public UserRequestParameter(int pageNumber = 1, int pageSize = 5)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
