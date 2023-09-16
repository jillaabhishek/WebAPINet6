using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Responses
{
    public sealed class CompanyBadRequestResponse : ApiBadRequestResponse
    {
        public CompanyBadRequestResponse(string message) : base(message)
        {
        }
    }
}
