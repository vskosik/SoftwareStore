using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SoftwareStore.Models;

namespace SoftwareStore.Repository
{
    public interface IHistoryRepository : IRepository<History>
    {
        IEnumerable<History> Find(int userId);
    }
}
