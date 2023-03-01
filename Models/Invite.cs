using System.ComponentModel.DataAnnotations;

namespace AstraBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string? InviteeFirstName { get; set; }
        
        [Required]
        [Display(Name ="Last Name")]
        public string? InviteeLastName { get; set; }

        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }

        public bool IsValid { get; set; }

        //Forign Keys
        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }
        

    }
}
