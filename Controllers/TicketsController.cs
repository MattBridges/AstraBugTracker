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
using System.ComponentModel.Design;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AstraBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
		private readonly IBTTicketService _ticketService;
        private readonly IBTProjectsService _projectsService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTTicketCommentService _ticketCommentService;
        private readonly ApplicationDbContext _context;
        private readonly IBTNotificationService _notificationService;



        public TicketsController(UserManager<BTUser> userManager,
                                 IBTFileService fileService,
                                 IBTTicketService ticketService,
                                 IBTProjectsService projectsService,
                                 IBTRolesService rolesService,
                                 IBTTicketHistoryService historyService,
                                 IBTTicketCommentService ticketCommentService,
                                 ApplicationDbContext context,IBTNotificationService notificationService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _ticketService = ticketService;
            _projectsService = projectsService;
            _rolesService = rolesService;
            _historyService = historyService;
            _ticketCommentService = ticketCommentService;
            _context = context;
            _notificationService = notificationService;
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
                BTUser? btUser = await _userManager.FindByIdAsync(userId!);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);                
                await _ticketService.AddDeveloperToTicketAsync(viewModel.Ticket?.Id, viewModel.DeveloperId);
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket,newTicket,userId!);
                //await _historyService.AddHistoryAsync(null!, newTicket!, userId!);

                Notification? notification = new()
                {
                    TicketId = viewModel.Ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"Ticket : {viewModel.Ticket.Title} was assigned by {btUser?.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = viewModel.DeveloperId,
                    ProjectId = viewModel.Ticket!.ProjectId,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New Developer Assigned");

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

        // GET: My Tickets
        public async Task<IActionResult> MyTickets()
        {
            BTUser? user = await _userManager.GetUserAsync(User);
            IEnumerable<Ticket> tickets = await _ticketService.GetIncompleteTicketsAsync(user!.Id);



            return View(tickets);
        }
        // GET:Unassigned Tickets
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            BTUser? user = await _userManager.GetUserAsync(User);
            IEnumerable<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(user);



            return View(tickets);
        }
        // GET All Tickets: Tickets
        public async Task<IActionResult> Index(int? pageNum)
        {
            int companyId=User.Identity!.GetCompanyId();
            IEnumerable<Ticket> tickets =await _ticketService.GetIncompleteTicketsAsync(companyId);                
            
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int? companyId= User.Identity!.GetCompanyId();
            Ticket ticket = await _ticketService.GetTicketAsync(id, companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            IEnumerable<TicketComment> comments = await _ticketCommentService.GetTicketCommentsAsync(ticket.Id);
           
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

				ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow.ToLocalTime());
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
            //Structure the ticket comment then add it
            TicketComment ticketComment = new TicketComment();
            ticketComment.TicketId = Id;
            ticketComment.Comment = comment;
            ticketComment.UserId= _userManager.GetUserId(User);
            ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow.ToLocalTime());
            await _ticketCommentService.AddTicketCommentAsync(ticketComment);

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
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            ViewData["DeveloperUserId"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer),companyId), "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(await _projectsService.GetActiveProjectsAsync(companyId, null), "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId), "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");
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
                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow.ToLocalTime());
                ticket.SubmitterUserId = _userManager.GetUserId(User);


                await _ticketService.AddTicketAsync(ticket);

                //Add History Record
                int companyId = User.Identity!.GetCompanyId();
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                BTUser? projectManager = await _projectsService.GetProjectManagerAsync(ticket.ProjectId);
                BTUser? btUser = await _userManager.FindByIdAsync(userId!);

                await _historyService.AddHistoryAsync(null!, newTicket!, userId!);
                
                Notification? notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket : {ticket.Title} was created by {btUser}",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id,
                    ProjectId = ticket.ProjectId,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n=>n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationService.AdminNotificationAsync(notification, companyId);
                    await _notificationService.SendAdminNotificationAsync(notification, "New Project Ticket Added", companyId);
                }

                
                return RedirectToAction(nameof(Index));
            }
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int? userCompanyId = User.Identity!.GetCompanyId();
       
            Ticket ticket = await _ticketService.GetTicketAsync(id,userCompanyId);
                
            if (ticket == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            ViewData["DeveloperUserId"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName", ticket.DeveloperUser);
            ViewData["ProjectId"] = new SelectList(await _projectsService.GetActiveProjectsAsync(companyId, null), "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId), "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");

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
            ModelState.Remove("ProjectId");
            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();
                string? userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

              
                try
                {
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow.ToLocalTime());
                    ticket.SubmitterUserId = _userManager.GetUserId(User);
                    ticket.ProjectId = oldTicket.ProjectId;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                        return NotFound();
                  
                }

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket!, newTicket!, userId!);
                return RedirectToAction(nameof(Index));
            }
          
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            return View(await _ticketService.GetTicketAsync(id));
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
          
            Ticket? ticket = await _ticketService.GetTicketAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
                await _ticketService.UpdateTicketAsync(ticket);
            }
          
            return RedirectToAction(nameof(Index));
        }

        //private bool TicketExists(int id)
        //{
        //  return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
