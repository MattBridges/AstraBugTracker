using Microsoft.AspNetCore.Mvc.Rendering;

namespace AstraBugTracker.Models.ViewModels
{
    public class ProjectMemberViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? UserList { get; set; }
        public List<string>? SelectedMembers { get; set; }
    }
}
