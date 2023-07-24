using System.Collections.Generic;
using System.Threading.Tasks;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public interface IRepository<T> where T : BaseModel
    {
        // Create async
        Task<T> AddAsync(T model);

        // Read async
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        // Update
        Task<T> UpdateAsync(int id, T model);

        // Delete
        Task DeleteAsync(int id);

        Task<bool> IsExist(int id);
    }
}