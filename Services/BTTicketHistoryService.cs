using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Services.Interfaces;

namespace AstraBugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectsService _projectService;

        public BTTicketHistoryService(ApplicationDbContext context,IBTProjectsService projectsService)
        {
            _context = context;
            _projectService = projectsService;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            try
            {
                //Check if new ticket
                if(oldTicket== null && newTicket != null)
                {
                    //Ticket is new
                    TicketHistory? history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        UserId = userId,
                        Description = "New Ticket Created"
                    };
                    try
                    {
                        await _context.TicketHistory.AddAsync(history);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                else if(oldTicket!= null && newTicket!= null) 
                {
                    //Ticket has been modified
                    //Check ticket title
                    if(oldTicket.Title != newTicket.Title)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket title from {oldTicket.Title} to: {newTicket.Title}"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    //Check ticket description
                    if (oldTicket.Description != newTicket.Description)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket description"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    //Check ticket Priority Status
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Priority",
                            OldValue = oldTicket.TicketPriority?.Name,
                            NewValue = newTicket.TicketPriority?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket priority from {oldTicket.TicketPriority?.Name} to: {newTicket.TicketPriority?.Name}"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    //Check ticket Type
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Type",
                            OldValue = oldTicket.TicketType?.Name,
                            NewValue = newTicket.TicketType?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket type from {oldTicket.TicketType?.Name} to: {newTicket.TicketType?.Name}"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    //Check ticket Ticket Status
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Status",
                            OldValue = oldTicket.TicketStatus?.Name,
                            NewValue = newTicket.TicketStatus?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket status from {oldTicket.TicketStatus?.Name} to: {newTicket.TicketStatus?.Name}"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    //Check ticket developer
                    if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Developer User",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser?.FullName,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed ticket developer from {oldTicket.DeveloperUser?.FullName} to: {newTicket.DeveloperUser?.FullName}"
                        };

                        await _context.TicketHistory.AddAsync(history);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int? ticketId, string? model, string? userId)
        {
            try
            {
                Ticket? ticket= await _context.Tickets.FindAsync(ticketId);
                string description = model!.ToLower().Replace("ticket", "");
                description = $"New {description} added to ticket : {ticket!.Title}";

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    PropertyName = model,
                    OldValue = string.Empty,
                    NewValue = string.Empty,
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    UserId = userId,
                    Description = description
                };

                await _context.TicketHistory.AddAsync(history);
                await _context.SaveChangesAsync();  
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? companyId)
        {
            throw new NotImplementedException();
        }
    }
}
