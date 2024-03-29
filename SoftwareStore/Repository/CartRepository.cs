﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public class CartRepository : DbRepository<Cart>, ICartRepository
    {
        public CartRepository(SoftwareStoreContext context) : base(context) { }

        public async Task<Cart> Find(int userId, int productId)
        {
            return await entities.FirstOrDefaultAsync(cart => cart.UserId == userId && cart.ProductId == productId);
        }

        public IEnumerable<Cart> Find(int userId)
        {
            return entities.Where(cart => cart.UserId == userId);
        }
    }
}