﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstraBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        
        public byte[]? FileData { get; set; }
        
        public string? FileType { get; set; }
        
        [NotMapped]
        public IFormFile? FormFile { get; set; }

        //Forign Keys
        public int? TicketId { get; set;}
        
        [Required]
        public string? BTUserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        
        public virtual BTUser? BTUser { get; set; }
    }
}
