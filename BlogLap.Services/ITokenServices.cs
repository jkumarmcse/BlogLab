using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLap.Services
{
    public interface ITokenServices
    {
        public string CreateToken(ApplicationUserIdentity user);
    }
}
