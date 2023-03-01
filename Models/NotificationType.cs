using System.ComponentModel.DataAnnotations;

namespace AstraBugTracker.Models
{
    public class NotificationType
    {
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
