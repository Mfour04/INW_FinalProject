using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<Users>
    {
        Task<Users> CreateUser(Users users);
        Task<Users> GetByEmail(string email);
        Task<Users> GetById(string userId);
    }
}
