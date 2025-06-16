using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VeilingPlatform.Data;
using VeilingPlatform.DTO.Bidding;
using VeilingPlatform.Entities;
using VeilingPlatform.Exceptions;

public class AuctionItemRepository : IAuctionItemRepository
{
    private readonly ApplicationDbContext _context;

    public AuctionItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuctionItem> AddAsync(AuctionItem auctionItem)
    {
        _context.AuctionItems.Add(auctionItem);
        await _context.SaveChangesAsync();
        return auctionItem;
    }

    public async Task<bool> CancelAsync(int itemId, string userId)
    {
        var item = await _context.AuctionItems.FindAsync(itemId);
        if (item == null || item.SellerId != userId || item.EndDateTime < DateTime.Now)
        {
            return false;
        }

        item.Status = AuctionItemStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<IEnumerable<AuctionItem>> GetSoldItemsByUserIdAsync(string userId)
    {
        return await _context.AuctionItems
            .Where(ai => ai.SellerId == userId && ai.Status == AuctionItemStatus.Paid)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bid>> GetBidsByItemIdAsync(int itemId)
    {
        return await _context.Bids
            .Where(b => b.AuctionItemId == itemId)
            .ToListAsync();
    }

    public async Task<AuctionItem?> GetByIdAsync(int itemId)
    {
        return await _context.AuctionItems
                             .FirstOrDefaultAsync(ai => ai.Id == itemId);
    }



    // Bieden op items
    public async Task<IEnumerable<AuctionItem>> SearchItemsAsync(SearchAuctionItemsDto searchParams)
    {
        var items = _context.AuctionItems.Where(x => x.Status != AuctionItemStatus.Cancelled && x.EndDateTime >= DateTime.Now);

        if (searchParams.CategoryIds != null && searchParams.CategoryIds.Count > 0)
        {
            items = items.Where(x => searchParams.CategoryIds.Contains(x.CategoryId));
        }

        if (searchParams.MaxPrice.HasValue)
        {
            items = items.Where(x => x.Bids.Any(b => b.Amount <= searchParams.MaxPrice));
        }

        return await items.OrderBy(x => x.EndDateTime).ToListAsync();
    }

    public async Task<Bid> PlaceBidAsync(int itemId, string userId, decimal amount)
    {
        var auctionItem = await _context.AuctionItems
            .Include(ai => ai.Bids)
            .FirstOrDefaultAsync(ai => ai.Id == itemId && ai.SellerId != userId && ai.Status != AuctionItemStatus.Cancelled && ai.EndDateTime >= DateTime.Now);

        if (auctionItem == null)
        {
            throw new CustomException("Het item bestaat niet, is al afgelopen, is geannuleerd of je kunt niet bieden op je eigen item.");
        }

        var highestBid = auctionItem.Bids != null && auctionItem.Bids.Any()
            ? auctionItem.Bids.Max(b => b.Amount)
            : auctionItem.StartingPrice;

        if (amount <= highestBid * 1.05m)
        {
            throw new CustomException("Het nieuwe bod moet minimaal 5% hoger zijn dan het huidige hoogste bod.");
        }

        var bid = new Bid
        {
            // Afronden tot op €0,50
            Amount = Math.Ceiling(amount * 2) / 2,
            BidDateTime = DateTime.Now,
            BidderId = userId,
            AuctionItemId = itemId
        };

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        return bid;
    }


    public async Task<AuctionItem?> GetHighestBidAuctionItemAsync(int itemId)
    {
        return await _context.AuctionItems
                             .Where(ai => ai.Id == itemId)
                             .OrderByDescending(ai => ai.Bids.Max(b => b.Amount))
                             .Include(ai => ai.Bids)
                             .FirstOrDefaultAsync();
    }


    public async Task<bool> MakePaymentAsync(int itemId, string userId)
    {
        var auctionItem = await _context.AuctionItems
                                        .Include(ai => ai.Bids)
                                        .FirstOrDefaultAsync(ai => ai.Id == itemId);

        if (auctionItem == null || auctionItem.Status == AuctionItemStatus.Paid)
        {
            return false;
        }

        var highestBid = auctionItem.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        if (highestBid == null || highestBid.BidderId != userId)
        {
            return false;
        }

        auctionItem.Status = AuctionItemStatus.Paid;
        await _context.SaveChangesAsync();
        return true;
    }

}
