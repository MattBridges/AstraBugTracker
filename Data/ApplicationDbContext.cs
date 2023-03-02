using AstraBugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AstraBugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Company> Companies { get; set; } = default!;
        public virtual DbSet<ProjectPriority> ProjectPriorities { get; set; } = default!;
        public virtual DbSet<Project> Projects { get; set; } = default!;
        public virtual DbSet<TicketType> TicketTypes { get; set; } = default!;
        public virtual DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
        public virtual DbSet<TicketPriority> TicketPriorities { get; set; } = default!;
        public virtual DbSet<NotificationType> NotificationTypes { get; set; } = default!;
        public virtual DbSet<Ticket> Tickets { get; set; } = default!;
        public virtual DbSet<Invite> Invite { get; set; } = default!;
        public virtual DbSet<Notification> Notification { get; set; } = default!;
        public virtual DbSet<TicketAttachment> TicketAttachment { get; set; } = default!;
        public virtual DbSet<TicketComment> TicketComment { get; set; } = default!;
        public virtual DbSet<TicketHistory> TicketHistory { get; set; } = default!;

    }
}
