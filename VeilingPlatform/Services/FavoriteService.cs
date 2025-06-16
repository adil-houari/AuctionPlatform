using VeilingPlatform.Entities;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<Favorite> AddToFavorite(int auctionItemId, string userId)
    {
        return await _favoriteRepository.AddToFavorite(auctionItemId, userId);
    }

    public async Task<bool> RemoveFromFavorite(int favoriteId, string userId)
    {
        return await _favoriteRepository.RemoveFromFavorite(favoriteId, userId);
    }

    public async Task<IEnumerable<Favorite>> GetUserFavorites(string userId)
    {
        return await _favoriteRepository.GetUserFavorites(userId);
    }
}
