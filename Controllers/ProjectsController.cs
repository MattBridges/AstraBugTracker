using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Models.Enums;
using AstraBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using AstraBugTracker.Extensions;
using AstraBugTracker.Models.ViewModels;
using System.Collections;
using Microsoft.AspNetCore.Identity;

namespace AstraBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectsService _projectsService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public ProjectsController(ApplicationDbContext context, IBTFileService fileService, IBTProjectsService projectsService,IBTRolesService rolesService,UserManager<BTUser> userManager)
        {
            _context = context;
            _fileService = fileService;
            _projectsService = projectsService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            //Validate Id
            if(id == null)
            {
                return NotFound();
            }

            //Get Company Id
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectsService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new(){
                Project = await _projectsService.GetProjectByIdAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if(!string.IsNullOrEmpty(viewModel.PMId))
            {
                await _projectsService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);
                return RedirectToAction("Details",new {id = viewModel.Project?.Id });
            }
            //If Null reset the view to get it corrected
            ModelState.AddModelError("PMID", "No Project Manager Chosen. Please Select a PM");
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectsService.GetProjectManagerAsync(viewModel.Project?.Id);

            viewModel.Project = await _projectsService.GetProjectByIdAsync(viewModel.Project?.Id, companyId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM.Id);
            viewModel.PMId = currentPM?.Id;

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Project Manager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectsService.GetProjectByIdAsync(id, companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            List<BTUser> userList = submitters.Concat(developers).ToList();
            List<string>? currentMembers = project.Members.Select(m=>m.Id).ToList();

            ProjectMemberViewModel viewModel = new()
            {
                Project = project,
                UserList = new MultiSelectList(userList,"Id","FullName",currentMembers),
            };

            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Project Manager")]
        public async Task<IActionResult> AssignProjectMembers(ProjectMemberViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (viewModel.SelectedMembers !=null)
            {
                //Remove current members
                await _projectsService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

                //Add newly selected members
                await _projectsService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project.Id, companyId);

                return RedirectToAction(nameof(Details), new {id = viewModel.Project!.Id});
            }

            ModelState.AddModelError("SelectedMembers", "No Users Chosen. Please Select Users");
            //Reset Form
            viewModel.Project = await _projectsService.GetProjectByIdAsync(viewModel.Project!.Id, companyId);
            List<string>? currentMembers = viewModel.Project.Members.Select(m => m.Id).ToList();
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> userList = submitters.Concat(developers).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> MyProjects()
        {
            int? companyId = User.Identity!.GetCompanyId();
            BTUser? user =await _userManager.GetUserAsync(User);
            return View(await _projectsService.GetActiveProjectsAsync(companyId,user));
        }

        public async Task<IActionResult> MyArchivedProjects()
        {
            int? companyId = User.Identity!.GetCompanyId();
            BTUser? user = await _userManager.GetUserAsync(User);
            return View(await _projectsService.GetActiveProjectsAsync(companyId, user, true));
        }

        public async Task<IActionResult> CompanyArchivedProjects()
        {
            int? companyId = User.Identity!.GetCompanyId();
            return View(await _projectsService.GetActiveProjectsAsync(companyId,null,true));
        }



        // GET: All Projects
        public async Task<IActionResult> Index()
        {
            int? companyId = User.Identity!.GetCompanyId();
            return View(await _projectsService.GetActiveProjectsAsync(companyId, null, false));
        }

        // GET: My Projects
        public async Task<IActionResult> ViewMyProjects()
        {
            int companyId = User.Identity!.GetCompanyId();
            BTUser? user = await _userManager.GetUserAsync(User);
            
            IEnumerable<Project> userProjects = await _projectsService.GetActiveProjectsAsync(companyId, user);

            return View(userProjects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectsService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            IEnumerable<Company> companies = _context.Companies.ToList();  
            int? companyId = User.Identity?.GetCompanyId();

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            ViewData["PMUserIds"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId!.Value), "Id", "FullName");


            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived,CompanyId, PMUserId")] Project project, string PMUserId)
        {
            ModelState.Remove("CompanyId");
            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();

                project.CompanyId = companyId;
                //Reformat Dates
                project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

             

                //Image Service
                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _projectsService.AddProjectAsync(project);

                //Assign Project Manager
                await _projectsService.AddProjectManagerAsync(PMUserId, project.Id);

                return RedirectToAction(nameof(Index));
            }
           
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived,CompanyId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            ModelState.Remove("CompanyId");
            if (ModelState.IsValid)
            {
                try
                {
                    int companyId = User.Identity!.GetCompanyId();

                    project.CompanyId = companyId;
                    //Reformat Dates
                    project.Created = DataUtility.GetPostGresDate(project.Created);
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                    //Image Service
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
          
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }





        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project? project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.Archived = !project.Archived;
                _context.Projects.Update(project);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

         
     

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
