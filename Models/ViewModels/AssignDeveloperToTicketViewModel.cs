using Microsoft.AspNetCore.Mvc.Rendering;

namespace AstraBugTracker.Models.ViewModels
{
    public class AssignDeveloperToTicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DeveloperList { get; set; }
        public string? DeveloperId { get; set; }
    }
}
