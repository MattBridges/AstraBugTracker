using System.ComponentModel.DataAnnotations;

namespace AstraBugTracker.Models
{
    public class EmailData
    {
        [Required]
        public string? EmailSenderAddress { get; set; }
        [Required]
        public string? EmailSenderName { get; set; }
        [Required]
        public string? EmailBody { get; set; }
        
        public string? EmailSubject { get; set; }        
    }
}
