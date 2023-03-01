﻿using System.ComponentModel.DataAnnotations;
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
        public DateTime Created { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectPriorityId { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public bool Archived { get; set; }


        //Forign Keys
        public virtual int CompanyId { get; set; }


        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual string? ProjectPriority { get;set; }
        public virtual ICollection<BTUser> Members { get;set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket>? Tickets { get;set; } = new HashSet<Ticket>();
    }
}