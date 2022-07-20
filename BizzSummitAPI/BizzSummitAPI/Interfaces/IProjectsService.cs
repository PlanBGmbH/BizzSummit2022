using System.Collections.Generic;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public interface IProjectsService
    {
        Task<IEnumerable<Project>> GetProjectsAsync(string query);

        Task AddProjectAsync(Project project);

        Task DeleteProjectAsync(string id);

        Task UpdateProjectAsync(Project project);
    }
}
