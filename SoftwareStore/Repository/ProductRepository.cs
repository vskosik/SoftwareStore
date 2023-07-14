using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public class ProductRepository : DbRepository<Product>
    {
        public ProductRepository(SoftwareStoreContext context) : base(context) { }

        public new async Task<List<Product>> GetAllAsync()
        {
            return await entities.Include("Vendor").ToListAsync();
        }
    }
}