using AstraBugTracker.Extensions;
using AstraBugTracker.Models;
using AstraBugTracker.Models.ChartModels;
using AstraBugTracker.Models.Enums;
using AstraBugTracker.Models.ViewModels;
using AstraBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AstraBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IBTProjectsService _projectService;
		private readonly IBTTicketService _ticketService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _roleService;



		public HomeController(ILogger<HomeController> logger, IBTProjectsService projectService, IBTTicketService ticketService, IBTCompanyService companyService, IBTRolesService roleService)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyService = companyService;
            _roleService = roleService;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {

                return View("LandingPage");
            }
        }

        public IActionResult LandingPage()
        {
            return View();
        }

		public async Task<IActionResult> Dashboard()
		{
			DashboardViewModel viewModel= new DashboardViewModel();
            int? companyId = User.Identity!.GetCompanyId();
            viewModel.Projects = await _projectService.GetActiveProjectsAsync(companyId);
            viewModel.Tickets = await _ticketService.GetIncompleteTicketsAsync(companyId);
            viewModel.Members = await _companyService.GetMembersAsync(companyId);

            
            
            return View(viewModel);
		}

		[HttpPost]
		public async Task<JsonResult> GglProjectTickets()
		{
			int? companyId = User.Identity!.GetCompanyId();

			IEnumerable<Project> projects = await _projectService.GetActiveProjectsAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "ProjectName", "TicketCount" });

			foreach (Project prj in projects)
			{
				chartData.Add(new object[] { prj.Name!, prj.Tickets.Count() });
			}

			return Json(chartData);
		}


		[HttpPost]
		public async Task<JsonResult> GglProjectPriority()
		{
			int? companyId = User.Identity!.GetCompanyId();

			IEnumerable<Project> projects = await _projectService.GetActiveProjectsAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "Priority", "Count" });


			foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
			{
				int priorityCount = (await _projectService.GetAllProjectSByPriorityAsync(companyId, priority)).Count();
				chartData.Add(new object[] { priority, priorityCount });
			}

			return Json(chartData);
		}


        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetActiveProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer),companyId)).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();


            return Json(amChartData.Data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}