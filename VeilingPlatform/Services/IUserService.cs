using System.Threading.Tasks;
using VeilingPlatform.Entities;

public interface IUserService
{
    Task<string> GetSubscriptionTypeAsync(string userId);
}
