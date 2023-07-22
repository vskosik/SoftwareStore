using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public class HistoryRepository : DbRepository<History>
    {
        public HistoryRepository(SoftwareStoreContext context) : base(context) { }

        public IEnumerable<History> Find(int userId)
        {
            return entities.Where(history => history.UserId == userId);
        }
    }
}