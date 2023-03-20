namespace AstraBugTracker.Models.ViewModels
{
	public class DashboardViewModel
	{

		public Company? Company { get; set; }
		public IEnumerable<Project>? Projects { get; set; }
		public IEnumerable<Ticket>? Tickets { get; set; }
		public IEnumerable<BTUser>? Members { get; set; }

	}
}
