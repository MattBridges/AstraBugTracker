using AstraBugTracker.Models;

namespace AstraBugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification);

        public Task AdminNotificationAsync(Notification? notification, int? companyId);

        public Task<List<Notification>> GetNotificationsByUserAsync(string? userId);

        public Task<bool> SendAdminNotificationAsync(Notification? notification, string? emailSubject, int? companyId);

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject);

    }
}
