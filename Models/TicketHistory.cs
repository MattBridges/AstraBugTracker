using System.ComponentModel.DataAnnotations;

namespace AstraBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        [Display(Name = "Property Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? PropertyName { get; set; }
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        [Display(Name = "Old Value")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? OldValue { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        [Display(Name = "New Value")]
        public string? NewValue { get; set; }
        [Required]
        public string? UserId { get; set; }


        //Forign Keys
        public int TicketId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
