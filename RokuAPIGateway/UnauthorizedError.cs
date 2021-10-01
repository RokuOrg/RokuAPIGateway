using Ocelot.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RokuAPIGateway
{
    public class UnauthorizedError : Error
    {
        public UnauthorizedError(string message, OcelotErrorCode code, int httpStatusCode) : base(message, code, httpStatusCode)
        {
        }
    }
}
