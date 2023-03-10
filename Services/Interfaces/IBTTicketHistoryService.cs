using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);

        Task AddHistoryAsync(int? ticketId, string? model, string? userId);

        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId);

        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? companyId);
    }
}
