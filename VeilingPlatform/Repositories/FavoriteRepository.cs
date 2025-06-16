using Microsoft.EntityFrameworkCore;
using VeilingPlatform.Data;
using VeilingPlatform.Entities;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _context;

    public FavoriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Favorite> AddToFavorite(int auctionItemId, string userId)
    {
        var favorite = new Favorite { AuctionItemId = auctionItemId, UserId = userId };
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> RemoveFromFavorite(int favoriteId, string userId)
    {
        var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);
        if (favorite == null) return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Favorite>> GetUserFavorites(string userId)
    {
        return await _context.Favorites.Where(f => f.UserId == userId).ToListAsync();
    }
}
