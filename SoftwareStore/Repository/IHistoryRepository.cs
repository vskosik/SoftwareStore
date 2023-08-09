using System.Collections.Generic;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public interface IHistoryRepository : IRepository<History>
    {
        IEnumerable<History> Find(int userId);
    }
}
