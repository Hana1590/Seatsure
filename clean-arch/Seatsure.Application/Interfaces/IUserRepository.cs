using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatsure.Domain; 

namespace Seatsure.Application.Interfaces;
public interface IUserRepository
{
       

      Task<User?> GetByIdAsync(Guid id);

      Task<User?> GetByEmailAsync(string email); 

      

      Task AddAsync(User user);
}

