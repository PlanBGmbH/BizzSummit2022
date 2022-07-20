using System.Collections.Generic;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public interface IResourcesService
    {
        Task<IEnumerable<Resource>> GetResourcesAsync(string query);

        Task AddResourceAsync(Resource resource);
        
        Task DeleteResourceAsync(string id);

        Task UpdateResourceAsync(Resource resource);
    }
}
