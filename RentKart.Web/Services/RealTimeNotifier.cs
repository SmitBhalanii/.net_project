using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RentKart.Core.Interfaces;
using RentKart.Web.Hubs;

namespace RentKart.Web.Services
{
    public class RealTimeNotifier : IRealTimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public RealTimeNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string? url = null)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new 
            {
                title = title,
                message = message,
                url = url
            });
        }

        public async Task UpdateUnreadCountAsync(string userId, int count)
        {
            await _hubContext.Clients.User(userId).SendAsync("UpdateUnreadCount", count);
        }
    }
}