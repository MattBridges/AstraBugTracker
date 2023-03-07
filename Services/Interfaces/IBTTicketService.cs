using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
		public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
    }
}
