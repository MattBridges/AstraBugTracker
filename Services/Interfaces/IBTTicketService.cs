using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
		public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        public Task<Ticket> GetTicketByIdAsync(int? id);

        public Task<BTUser> GetCurrentDeveloperAsync(int? ticketId);

        public Task<bool> AddDeveloperToTicketAsync(int? ticketId, string userId);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);

        //public Task<IEnumerable<Ticket>> GetUnassignedTickets(int? companyId);


    }
}
