using SoftwareStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareStore.Repository
{
    public interface IProductRepository : IRepository<Product>
    {
        new Task<List<Product>> GetAllAsync();

        new Task<Product> GetByIdAsync(int id);

        new Task<Product> UpdateAsync(int id, Product model);
    }
}
