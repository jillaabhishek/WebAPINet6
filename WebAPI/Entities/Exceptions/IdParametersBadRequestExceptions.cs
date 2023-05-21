using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public class IdParametersBadRequestExceptions : BadRequestException
    {
        public IdParametersBadRequestExceptions()
            :base("Parameter ids is null")
        {

        }
    }
}
