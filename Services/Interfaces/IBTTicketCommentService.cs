using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTTicketCommentService
    {
        //Add A Comment

        //Remove A Comment

        //Get Ticket Comments
        public Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int? id);

        public Task AddTicketCommentAsync(TicketComment comment);        
    }
}
