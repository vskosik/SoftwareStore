using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareStore.Repository
{
    public class DbRepository<T> : IRepository<T> where T : BaseModel
    {
        protected readonly SoftwareStoreContext context;
        protected readonly DbSet<T> entities;

        public DbRepository(SoftwareStoreContext context)
        {
            this.context = context;
            entities = this.context.Set<T>();
        }

        public async Task<T> AddAsync(T model)
        {
            if (model == null)
            {
                return null;
            }
            await entities.AddAsync(model);
            await context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await entities.FindAsync(id);
            if (item == null)
            {
                return;
            }
            entities.Remove(item);
            await context.SaveChangesAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await entities.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await entities.FindAsync(id);
        }

        public async Task<T> UpdateAsync(int id, T model)
        {
            var item = await GetByIdAsync(id);
            if (item == null || model == null)
            {
                return null;
            }
            item = model;
            item.Id = id;
            context.Entry(item).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return item;
        }
    }
}
