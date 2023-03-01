﻿using System.ComponentModel.DataAnnotations;

namespace AstraBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Comment { get; set; }
        public DateTime Created { get; set; }

        //Forign Keys
        public virtual int TicketId { get; set; }
        public virtual string? UserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }

    }
}
