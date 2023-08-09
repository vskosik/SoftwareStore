using System.Collections.Generic;
using System.Linq;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public class HistoryRepository : DbRepository<History>, IHistoryRepository
    {
        public HistoryRepository(SoftwareStoreContext context) : base(context) { }

        public IEnumerable<History> Find(int userId)
        {
            return entities.Where(history => history.UserId == userId);
        }
    }
}