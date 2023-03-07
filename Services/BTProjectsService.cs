using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstraBugTracker.Services
{
    public class BTProjectsService : IBTProjectsService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectsService(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task DeleteProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                _context.Projects.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            try
            {
                IEnumerable<Project>? projects = await _context.Projects.ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task GetProjectsAsync(BTUser user)
        {
            throw new NotImplementedException();
        }

        public Task GetProjectsAsync(Company company)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

    }
}
