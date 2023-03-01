using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstraBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }
        
        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        public int ProjectPriorityId { get; set; }
        
        public byte[]? ImageFileData { get; set; }
        
        public string? ImageFileType { get; set; }
        
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        
        public bool Archived { get; set; }


        //Forign Keys
        public int CompanyId { get; set; }


        //Navigation Properties
        public virtual Company? Company { get; set; }
        
        public virtual ProjectPriority? ProjectPriority { get;set; }
        
        public virtual ICollection<BTUser> Members { get;set; } = new HashSet<BTUser>();
        
        public virtual ICollection<Ticket> Tickets { get;set; } = new HashSet<Ticket>();
    }
}