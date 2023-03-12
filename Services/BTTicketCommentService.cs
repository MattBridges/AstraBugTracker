using AstraBugTracker.Data;
using AstraBugTracker.Models;
using AstraBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AstraBugTracker.Services
{
    public class BTTicketCommentService: IBTTicketCommentService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketCommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                _context.TicketComment.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int? id)
        {
            try
            {
                return await _context.TicketComment.Where(t => t.TicketId == id).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
