using AstraBugTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTProjectsService
    {
        public Task AddProjectAsync(Project project);
        public Task AddMembersToProjectAsync(IEnumerable<string> userId, int? projectId, int? companyId);
        public Task<bool> AddMemberToProjectAsync(int? projectId, BTUser? member);
        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
              
        public Task DeleteProjectAsync(Project project);

        public Task RemoveMembersFromProjectAsync(int?projectId, int? companyId);
        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);

        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);
        public Task<BTUser> GetProjectManagerAsync(int? projectId);
        public Task<IEnumerable<Project>> GetActiveProjectsAsync(int? companyId, BTUser? user = null, bool? isArchived=false);
        public Task<IEnumerable<Project>> GetAllProjectSByPriorityAsync(int? companyId, string? priority);


	}
}
