using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VeilingPlatform.DTO.Bidding;

[ApiController]
[Route("api/auction")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionItemService _auctionItemService;

    public AuctionController(IAuctionItemService auctionItemService)
    {
        _auctionItemService = auctionItemService;
    }

    // GET: /api/auction/items/search
    [HttpGet("items/search")]
    public async Task<IActionResult> SearchItems([FromQuery] SearchAuctionItemsDto searchParams)
    {
        var items = await _auctionItemService.SearchItemsAsync(searchParams);
        return Ok(items);
    }

    // POST: /api/auction/items/{itemId}/bids
    [Authorize]
    [HttpPost("items/{itemId}/bids")]
    public async Task<IActionResult> PlaceBid(int itemId, [FromBody] PlaceBidDto bidDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var bid = await _auctionItemService.PlaceBidAsync(itemId, userId, bidDto.Amount);
        return Ok(bid); 
    }

    // GET: /api/auction/buyers/{userId}/items
    [Authorize]
    [HttpGet("buyers/{userId}/items")]
    public async Task<IActionResult> GetPurchasedItems(string userId)
    {
        if (User.FindFirst("sub")?.Value != userId && !User.IsInRole("Admin")) 
        {
            return Forbid();
        }

        var items = await _auctionItemService.GetSoldItemsAsync(userId);
        return Ok(items);
    }

    // POST: /api/auction/buyers/{userId}/items/{itemId}/payment
    [Authorize]
    [HttpPost("buyers/{userId}/items/{itemId}/payment")]
    public async Task<IActionResult> MakePayment(string userId, int itemId)
    {
        if (User.FindFirst("sub")?.Value != userId)
        {
            return Forbid();
        }

        var success = await _auctionItemService.MakePaymentAsync(itemId, userId);
        if (!success)
        {
            return BadRequest("Betaling mislukt of niet toegestaan.");
        }

        return Ok(new { message = "Betaling geslaagd." });
    }
}
