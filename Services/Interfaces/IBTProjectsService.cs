using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTProjectsService
    {
        public Task AddProjectAsync(Project project);
        //public Task<IEnumerable<Project>> GetProjectsAsync();
        //public Task GetProjectsAsync(BTUser user);
        //public Task GetProjectsAsync(Company company);
        //public Task UpdateProjectAsync(Project project);
        //public Task DeleteProjectAsync(Project project);

        public Task<bool> AddMemberToProjectAsync(int? projectId, BTUser? member);
        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);
        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
        public Task<BTUser> GetProjectManagerAsync(int? projectId);


        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);
    }
}
