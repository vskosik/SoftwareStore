using System.Threading.Tasks;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> IsExist(string email);

        Task<User> Find(string email);
    }
}
