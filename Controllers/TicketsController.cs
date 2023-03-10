using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AstraBugTracker.Data;
using AstraBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using AstraBugTracker.Services;
using CodeIT.Services;
using AstraBugTracker.Services.Interfaces;
using AstraBugTracker.Extensions;
using AstraBugTracker.Models.Enums;
using AstraBugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AstraBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
		private readonly IBTTicketService _ticketService;
        private readonly IBTProjectsService _projectsService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketHistoryService _historyService;


		public TicketsController(ApplicationDbContext context, 
                                 UserManager<BTUser> userManager, 
                                 IBTFileService fileService, 
                                 IBTTicketService ticketService, 
                                 IBTProjectsService projectsService, 
                                 IBTRolesService rolesService, 
                                 IBTTicketHistoryService historyService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _ticketService = ticketService;
            _projectsService = projectsService;
            _rolesService = rolesService;
            _historyService = historyService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            //Validate Id
            if (id == null)
            {
                return NotFound();
            }

            //Get Company Id
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            BTUser? currentDeveloper = await _ticketService.GetCurrentDeveloperAsync(id);
            AssignDeveloperToTicketViewModel viewModel = new()
            {
                Ticket = await _ticketService.GetTicketByIdAsync(id),
                DeveloperList = new SelectList(developers, "Id", "FullName", currentDeveloper?.Id),
                DeveloperId = currentDeveloper?.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Project Manager")]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperToTicketViewModel viewModel)
        {
                int companyId= User.Identity!.GetCompanyId();
            string? userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(viewModel.DeveloperId))
            {
                //Get Snapshot

                //Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketId, companyId)
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);

                
                await _ticketService.AddDeveloperToTicketAsync(viewModel.Ticket?.Id, viewModel.DeveloperId);

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket,newTicket,userId!);
                return RedirectToAction("Details", new { id = viewModel.Ticket?.Id });
            }
            //If Null reset the view to get it corrected
            ModelState.AddModelError("DevloperId", "No Developer Chosen. Please Select a Developer");

            IEnumerable<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            BTUser? currentDeveloper = await _ticketService.GetCurrentDeveloperAsync(viewModel.Ticket!.Id);
            
            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket?.Id);
            viewModel.DeveloperList = new SelectList(developers, "Id", "FullName", currentDeveloper.Id);
            viewModel.DeveloperId = currentDeveloper?.Id;

            return View(viewModel);
        }
        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            IEnumerable<Ticket> tickets = await _context.Tickets.Where(t=>t.Archived ==false).ToListAsync();
            
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t=>t.Attachments)
                .Include(t=>t.History)
                .FirstOrDefaultAsync(m => m.Id == id);

            IEnumerable<TicketComment> comments = await _context.TicketComment.Where(t=>t.TicketId== id).ToListAsync();
            ViewData["Comments"] = comments;
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;
            ModelState.Remove("BTUserId");
			if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
				ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

				await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                //Add History
                await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId);

                statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}

			return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment(int Id, string comment)
        {
            TicketComment ticketComment = new TicketComment();
            ticketComment.TicketId = Id;
            ticketComment.Comment = comment;
            ticketComment.UserId= _userManager.GetUserId(User);
            ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
            _context.TicketComment.Add(ticketComment);
            await _context.SaveChangesAsync();

            //Add History
            await _historyService.AddHistoryAsync(Id, nameof(TicketComment),ticketComment.UserId);
            return RedirectToAction("Details",new { id = ticketComment.TicketId });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string? fileName = ticketAttachment.FileName;
            byte[]? fileData = ticketAttachment.FileData;
            string? ext = Path.GetExtension(fileName)?.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData!, $"application/{ext}");
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            string? userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticket.SubmitterUserId = _userManager.GetUserId(User);


                _context.Add(ticket);

                //Add History Record
                int companyId = User.Identity!.GetCompanyId();
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null!, newTicket!, userId!);
                //TODO: Notification


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName",ticket.DeveloperUser);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();
                string? userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

              
                try
                {
                    ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                    ticket.SubmitterUserId = _userManager.GetUserId(User);
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket!, newTicket!, userId!);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            Ticket? ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
                _context.Tickets.Update(ticket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
