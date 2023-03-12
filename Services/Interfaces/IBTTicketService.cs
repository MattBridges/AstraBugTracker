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

        public Task<IEnumerable<Ticket>> GetIncompleteTicketsAsync(int? companyId);

        public Task<Ticket> GetTicketAsync(int? id);

        public Task AddTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync();

        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();

        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();





    }
}
