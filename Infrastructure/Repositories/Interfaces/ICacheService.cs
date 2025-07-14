using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICacheService
    {
        Task<bool> Exists(string key);
        Task Set(string key, string value, TimeSpan? expiry = null);
        bool TryGetValue(string key, out string value);
    }

}
