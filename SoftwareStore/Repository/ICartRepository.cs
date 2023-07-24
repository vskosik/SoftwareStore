using SoftwareStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareStore.Repository
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart> Find(int userId, int productId);

        IEnumerable<Cart> Find(int userId);
    }
}
