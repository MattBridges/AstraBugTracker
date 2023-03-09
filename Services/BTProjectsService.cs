using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AstraBugTracker.Models.Enums;

namespace AstraBugTracker.Services
{
    public class BTProjectsService : IBTProjectsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        
        public BTProjectsService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }


        //private readonly ApplicationDbContext _context;

        //public BTProjectsService(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        //public async Task DeleteProjectAsync(Project project)
        //{
        //    try
        //    {
        //        project.Archived = true;
        //        _context.Projects.Update(project);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<Project>> GetProjectsAsync()
        //{
        //    try
        //    {
        //        IEnumerable<Project>? projects = await _context.Projects.ToListAsync();
        //        return projects;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public Task GetProjectsAsync(BTUser user)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task GetProjectsAsync(Company company)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task UpdateProjectAsync(Project project)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<bool> AddMemberToProjectAsync(int? projectId, BTUser? member)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId!.Value, member!.CompanyId);
                bool IsOnProject = project.Members.Any(m=>m.Id== member.Id);

                if(!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                if (currentPM != null) {
                    await RemoveProjectManagerAsync(projectId);
                }
                try
                {
                    await AddMemberToProjectAsync(projectId, selectedPM!);
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await _context.Projects.Where(p => p.CompanyId ==companyId)
                                       .Include(p => p.Company)
                                       .Include(p => p.Members)
                                       .Include(p => p.ProjectPriority)
                                       .Include(p => p.Tickets)
                                           .ThenInclude(t => t.DeveloperUser)
                                       .Include(p => p.Tickets)
                                           .ThenInclude(t => t.SubmitterUser)
                                       .FirstOrDefaultAsync(m => m.Id == projectId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p=>p.Members).FirstOrDefaultAsync(p=>p.Id== projectId);

                foreach(BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member,nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);
                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach(BTUser member in project!.Members)
                {
                    if(await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach(string userId in userIds)
                {
                    BTUser? btUser = await _context.Users.FindAsync(userId);
                    
                    if(project !=null && btUser!= null)
                    {
                        bool IsOnProject = project.Members.Any(m=>m.Id == btUser.Id);
                        if (!IsOnProject)
                        {
                            project.Members.Add(btUser);
                        }
                        
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId,companyId);
                foreach(BTUser member in project.Members)
                {
                    if(!await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
