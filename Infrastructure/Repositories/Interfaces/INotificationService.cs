using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, string type);
    }
}
