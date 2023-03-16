using AstraBugTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTProjectsService
    {
        public Task AddProjectAsync(Project project);
        public Task<IEnumerable<Project>> GetActiveProjectsAsync(int? companyId);
        public Task<IEnumerable<Project>> GetActiveProjectsAsync(int? companyId, string? userId);
       
        public Task DeleteProjectAsync(Project project);

        public Task AddMembersToProjectAsync(IEnumerable<string> userId, int? projectId, int? companyId);
        public Task RemoveMembersFromProjectAsync(int?projectId, int? companyId);

        public Task<bool> AddMemberToProjectAsync(int? projectId, BTUser? member);
        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);
        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
        public Task<BTUser> GetProjectManagerAsync(int? projectId);


        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);


        /*public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()*/

    }
}
