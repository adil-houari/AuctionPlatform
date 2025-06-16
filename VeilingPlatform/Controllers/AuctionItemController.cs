using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VeilingPlatform.DTO.AuctionItems;
using VeilingPlatform.Entities;
using VeilingPlatform.Extensions;

namespace VeilingPlatform.Controllers
{
    [Route("api/auction")]
    [ApiController]
    [Authorize]
    public class AuctionItemController : ControllerBase
    {
        private readonly IAuctionItemService _auctionItemService;

        public AuctionItemController(IAuctionItemService auctionItemService)
        {
            _auctionItemService = auctionItemService;
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateAuctionItem(CreateAuctionItemDto dto)
        {
            var userId = User.GetUserId(); 
            var auctionItem = await _auctionItemService.CreateAuctionItemAsync(dto, userId);
            return CreatedAtAction(nameof(GetAuctionItem), new { itemId = auctionItem.Id }, auctionItem);
        }

        [HttpDelete("items/{itemId}/cancel")]
        public async Task<IActionResult> CancelAuctionItem(int itemId)
        {
            var userId = User.GetUserId();
            var success = await _auctionItemService.CancelAuctionItemAsync(itemId, userId);
            if (!success)
            {
                return NotFound();
            }
            return Ok(new { message = "Veilingitem succesvol geannuleerd." });
        }


        [HttpGet("sellers/{userId}/items")]
        public async Task<IActionResult> GetSoldItems(string userId)
        {
            var currentUserId = User.GetUserId();
            if (userId != currentUserId)
            {
                return Unauthorized();
            }
            var soldItems = await _auctionItemService.GetSoldItemsAsync(userId);
            return Ok(soldItems);
        }

        [HttpGet("items/{itemId}/biddings")]
        public async Task<IActionResult> GetBidsForItem(int itemId)
        {
            var userId = User.GetUserId();
            var auctionItem = await _auctionItemService.GetAuctionItemByIdAsync(itemId);
            if (auctionItem == null)
            {
                return NotFound();
            }

            if (auctionItem.SellerId != userId)
            {
                return Forbid(); 
            }

            var bids = await _auctionItemService.GetBidsForItemAsync(itemId);
            return Ok(bids);
        }



        // Hele item bekijken (gemakkelijker om details de zien)
        [HttpGet("items/{itemId}")]
        public async Task<IActionResult> GetAuctionItem(int itemId)
        {
            var auctionItem = await _auctionItemService.GetAuctionItemByIdAsync(itemId);
            if (auctionItem == null)
            {
                return NotFound();
            }
            return Ok(auctionItem);
        }
    }
}
