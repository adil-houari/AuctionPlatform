using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeilingPlatform.DTO.Favorites;
using VeilingPlatform.Extensions;

namespace VeilingPlatform.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        // POST: api/Favorite
        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteDto addFavoriteDto)
        {
            var userId = User.GetUserId();
            var favorite = await _favoriteService.AddToFavorite(addFavoriteDto.AuctionItemId, userId);
            return CreatedAtAction(nameof(GetUserFavorites), new { userId = userId }, favorite);
        }

        // DELETE: api/Favorite/{favoriteId}
        [HttpDelete("{favoriteId}")]
        public async Task<IActionResult> RemoveFromFavorites(int favoriteId)
        {
            var userId = User.GetUserId();
            var success = await _favoriteService.RemoveFromFavorite(favoriteId, userId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET: api/Favorite
        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.GetUserId();
            var favorites = await _favoriteService.GetUserFavorites(userId);
            return Ok(favorites);
        }
    }
}
