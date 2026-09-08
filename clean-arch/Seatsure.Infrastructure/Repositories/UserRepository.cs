using Seatsure.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatsure.Application.Interfaces;
using Seatsure.Domain;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
namespace Seatsure.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
        private readonly AppDbContext _context;

    
    public UserRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
       
    }

    public async Task<User?> GetByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower()); 


    



    public async Task<User?> GetByIdAsync(Guid id) => await _context.Users.FindAsync(id);

}
