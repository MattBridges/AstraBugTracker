﻿using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace AstraBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTTicketService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetCurrentDeveloperAsync(int? ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t => t.Id == ticketId);

                BTUser? currentDeveloper = await _userManager.FindByIdAsync(ticket!.DeveloperUserId!);
                if (currentDeveloper == null)
                {
                    BTUser user = new BTUser();
                    user.FirstName = "Ticket";
                    user.LastName = "Unassigned";

                    return user;
                }

                return currentDeveloper!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachment
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int? id)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddDeveloperToTicketAsync(int? ticketId, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t => t.Id == ticketId);
                BTUser? selectedDeveloper = await _context.Users.FindAsync(userId);


                if (ticket == null)
                {
                    return false;
                }
                ticket!.DeveloperUser = selectedDeveloper;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                                 .Include(t => t.Project)
                                                    .ThenInclude(p => p!.Company)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.History)
                                                .Include(t => t.SubmitterUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetIncompleteTicketsAsync(int? companyId)
        {
            try
            {
                return await _context.Tickets.Where(t => t.Project!.CompanyId == companyId && t.Archived == false).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsync(int? id)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
               .Include(t => t.DeveloperUser)
               .Include(t => t.Project)
               .Include(t => t.SubmitterUser)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Include(t => t.TicketType)
               .Include(t => t.Attachments)
               .Include(t => t.History)
               .FirstOrDefaultAsync(m => m.Id == id);

                return ticket!;
            }

            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                IEnumerable<TicketPriority> priorities = await _context.TicketPriorities.ToListAsync();

                return priorities;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                IEnumerable<TicketStatus>? statuses = await _context.TicketStatuses.ToListAsync()!;
                return statuses;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                IEnumerable<TicketType> types = await _context.TicketTypes.ToListAsync();
                return (types);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
