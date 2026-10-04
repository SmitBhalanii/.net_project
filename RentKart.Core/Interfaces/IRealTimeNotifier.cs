using System.Threading.Tasks;

namespace RentKart.Core.Interfaces
{
    public interface IRealTimeNotifier
    {
        Task SendNotificationAsync(string userId, string title, string message, string? url = null);
        Task UpdateUnreadCountAsync(string userId, int count);
    }
}