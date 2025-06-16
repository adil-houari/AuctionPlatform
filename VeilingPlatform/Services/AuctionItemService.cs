using System.Collections.Generic;
using System;
using VeilingPlatform.DTO.AuctionItems;
using VeilingPlatform.DTO.Bidding;
using VeilingPlatform.Entities;
using VeilingPlatform.Exceptions;

public class AuctionItemService : IAuctionItemService
{
    private readonly IAuctionItemRepository _auctionItemRepository;
    private readonly IUserService _userService; // Een service die je user-gerelateerde informatie geeft

    public AuctionItemService(IAuctionItemRepository auctionItemRepository, IUserService userService)
    {
        _auctionItemRepository = auctionItemRepository;
        _userService = userService;
    }

    public async Task<AuctionItem> CreateAuctionItemAsync(CreateAuctionItemDto dto, string userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new CustomException("De naam van het veilingitem mag niet leeg zijn.");
        }

        if (dto.StartingPrice < 0)
        {
            throw new CustomException("De startprijs mag niet negatief zijn.");
        }


        //een buffer van 180 seconden (tijd om de gebruiken een item toe tevoegen) toe. 
        //Dit betekent dat als de starttijd 180 seconde in het verleden ligt op het moment dat de validatie wordt uitgevoerd, 
        //dit nog steeds als geldig wordt beschouwd.
        if (dto.StartDateTime < DateTime.UtcNow.AddSeconds(-180)) // Buffer van 1 seconde
        {
            throw new CustomException("De starttijd mag niet in het verleden liggen.");
        }
        var subscriptionType = await _userService.GetSubscriptionTypeAsync(userId);
        DateTime endDateTime;

        if (subscriptionType == "Gold" || subscriptionType == "Platinum")
        {
            // Controleer of de einddatum minimaal 12 uur na de startdatum is
            if (dto.EndDateTime.HasValue && dto.EndDateTime.Value < dto.StartDateTime.AddHours(12))
            {
                throw new CustomException("Eindtijdstip moet minimaal 12 uur na het starttijdstip liggen.");
            }
            endDateTime = dto.EndDateTime ?? dto.StartDateTime.AddHours(12);
        }
        else
        {
            // Voor Free gebruikers wordt de einddatum automatisch op 3 dagen na de startdatum ingesteld
            endDateTime = dto.StartDateTime.AddDays(3);
        }

        // Maak het nieuwe veilingitem
        var auctionItem = new AuctionItem
        {
            Name = dto.Name,
            Description = dto.Description,
            StartingPrice = dto.StartingPrice,
            StartDateTime = dto.StartDateTime,
            EndDateTime = endDateTime,
            SellerId = userId,
            Status = AuctionItemStatus.Initial,
            CategoryId = dto.CategoryId
        };

        return await _auctionItemRepository.AddAsync(auctionItem);
    }


    public async Task<bool> CancelAuctionItemAsync(int itemId, string userId)
    {
        var auctionItem = await _auctionItemRepository.GetByIdAsync(itemId);
        if (auctionItem == null)
        {
            throw new CustomException("Het veilingitem is niet gevonden.");
        }

        if (auctionItem.SellerId != userId)
        {
            throw new CustomException("Alleen de verkoper kan de verkoop annuleren.");
        }

        return await _auctionItemRepository.CancelAsync(itemId, userId);
    }



    public async Task<IEnumerable<AuctionItem>> GetSoldItemsAsync(string userId)
    {
        return await _auctionItemRepository.GetSoldItemsByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Bid>> GetBidsForItemAsync(int itemId)
    {
        var auctionItem = await _auctionItemRepository.GetByIdAsync(itemId);
        if (auctionItem == null)
        {
            throw new CustomException("Het veilingitem is niet gevonden en heeft daarom geen biedingen.");
        }

        return await _auctionItemRepository.GetBidsByItemIdAsync(itemId);
    }

    public async Task<AuctionItem> GetAuctionItemByIdAsync(int itemId)
    {
        return await _auctionItemRepository.GetByIdAsync(itemId);
    }



    // Bieden op items
    public async Task<IEnumerable<AuctionItem>> SearchItemsAsync(SearchAuctionItemsDto searchParams)
    {
        return await _auctionItemRepository.SearchItemsAsync(searchParams);
    }

    public async Task<Bid> PlaceBidAsync(int itemId, string userId, decimal amount)
    {
        // Eerst controleren we of de gebruiker niet op zijn eigen item biedt !! 
        var auctionItem = await GetAuctionItemByIdAsync(itemId);
        if (auctionItem == null)
        {
            throw new CustomException("Het veilingitem is niet gevonden.");
        }
        if (auctionItem.SellerId == userId)
        {
            throw new CustomException("Je kunt niet bieden op je eigen veilingitem.");
        }

        var highestBid = auctionItem.Bids.OrderByDescending(b => b.Amount).FirstOrDefault()?.Amount ?? auctionItem.StartingPrice;
        if (amount <= highestBid * 1.05m)
        {
            throw new CustomException("Het nieuwe bod moet minimaal 5% hoger zijn dan het huidige hoogste bod.");
        }

        amount = Math.Ceiling(amount * 2) / 2;

        // Plaats het bod
        return await _auctionItemRepository.PlaceBidAsync(itemId, userId, amount);
    }

    public async Task<bool> MakePaymentAsync(int itemId, string userId)
    {
        // Valideer dat de gebruiker de hoogste bieder is en het item nog niet betaald is
        var auctionItem = await _auctionItemRepository.GetHighestBidAuctionItemAsync(itemId);
        if (auctionItem == null || auctionItem.Status == AuctionItemStatus.Paid)
        {
            throw new CustomException("Betaling is niet mogelijk.");
        }

        var highestBid = auctionItem.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        if (highestBid == null || highestBid.BidderId != userId)
        {
            throw new CustomException("Je bent niet de hoogste bieder of het item is al betaald.");
        }

        // Voer de betaling uit en sla de wijzigingen op
        return await _auctionItemRepository.MakePaymentAsync(itemId, userId);
    }

}
