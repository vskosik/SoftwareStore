using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public class UserRepository : DbRepository<User>, IUserRepository
    {
        public UserRepository(SoftwareStoreContext context) : base(context) { }

        public async Task<bool> IsExist(string email)
        {
            return await entities.AnyAsync(user => user.Email == email);
        }

        public async Task<User> Find(string email)
        {
            return await entities.FirstOrDefaultAsync(user => user.Email == email);
        }
    }
}