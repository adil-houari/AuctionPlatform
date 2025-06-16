using Microsoft.AspNetCore.Identity;

public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GetSubscriptionTypeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var claims = await _userManager.GetClaimsAsync(user);
        var subscriptionClaim = claims.FirstOrDefault(c => c.Type == "SubscriptionType");

        // Default Free voor een user als er geen type is (handig)
        return subscriptionClaim?.Value ?? "Free"; 
    }
}