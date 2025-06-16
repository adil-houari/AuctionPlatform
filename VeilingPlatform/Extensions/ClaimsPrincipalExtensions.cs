using System.Security.Claims;

namespace VeilingPlatform.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // Voor controller (om id te hebben van de user en dit daarna te gebruiken)
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

    }
}
