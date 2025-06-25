using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.User
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserResponse User { get; set; }
    }

}
