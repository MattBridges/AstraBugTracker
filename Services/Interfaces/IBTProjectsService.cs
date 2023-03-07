using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTProjectsService
    {
        public Task AddProjectAsync(Project project);
        public Task<IEnumerable<Project>> GetProjectsAsync();
        public Task GetProjectsAsync(BTUser user);
        public Task GetProjectsAsync(Company company);
        public Task UpdateProjectAsync(Project project);
        public Task DeleteProjectAsync(Project project);
    }
}
