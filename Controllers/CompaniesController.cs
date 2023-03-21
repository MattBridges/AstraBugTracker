using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Models.ViewModels;
using AstraBugTracker.Extensions;
using AstraBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AstraBugTracker.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _roleService;
        private readonly IBTCompanyService _companyService;
        private readonly UserManager<BTUser> _userManager;

        public CompaniesController(ApplicationDbContext context, IBTRolesService rolesService, IBTCompanyService companyService, UserManager<BTUser> userManager)
        {
            _context = context;
            _roleService = rolesService;
            _companyService = companyService;
            _userManager = userManager;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return _context.Companies != null ?
                        View(await _context.Companies.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> viewModels = new();
            int? companyId = User.Identity!.GetCompanyId();

            List<BTUser> users = await _companyService.GetMembersAsync(companyId);
            List<IdentityRole> companyRoles = (await _roleService.GetRolesAsync()).ToList();

            foreach(BTUser user in users)
            {
                IEnumerable<string> currentRoles = await _roleService.GetUserRolesAsync(user);
                
                ManageUserRolesViewModel viewModel = new()
                {
                    BTUser = user,
                    Roles = new MultiSelectList(companyRoles,"Name","Name", currentRoles),
                };
                viewModels.Add(viewModel);
            }
            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {

            int? companyId = User.Identity!.GetCompanyId();
            BTUser? user = await _userManager.FindByIdAsync(viewModel.BTUser!.Id);
            if(user == null)
            {
                return NotFound();
            }
            List<string> currentRoles = (await _roleService.GetUserRolesAsync(user)).ToList();
            List<string> selectedRoles = viewModel.SelectedRoles!;

            await _roleService.RemoveUserFromRolesAsync(user, currentRoles);

            foreach(string role in selectedRoles)
            {
            await _roleService.AddUserToRoleAsync(user, role);
            }

            return RedirectToAction("Index", "Home");

        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id == null || _context.Companies == null)
            //{
            //    return NotFound();
            //}
            int? companyId = User.Identity!.GetCompanyId();
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
