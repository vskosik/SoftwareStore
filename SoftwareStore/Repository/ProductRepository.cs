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

        public new async Task<Product> GetByIdAsync(int id)
        {
            return await entities.Include("Vendor").FirstAsync(product => product.Id == id);
        }

        public new async Task<Product> UpdateAsync(int id, Product model)
        {
            var item = await GetByIdAsync(id);
            if (item == null || model == null)
            {
                return null;
            }
            
            item.Title = model.Title;
            item.Description = model.Description;
            item.Rate = model.Rate;
            item.Price = model.Price;
            item.VendorId = model.VendorId;
            item.Qty = model.Qty;

            await context.SaveChangesAsync();
            return item;
        }

    }
}