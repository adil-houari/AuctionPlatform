using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeilingPlatform.DTO.AuctionItems;
using VeilingPlatform.Entities;
using VeilingPlatform.Exceptions;

namespace VeilingPlatform.Tests
{

    // Arrange
    // Act
    // Assert

    public class AuctionItemServiceTests
    {

        // *************** Correcte aanmaak voor een 'Free' gebruiker ***************
        [Fact]
        public async Task CreateAuctionItemAsync_ForFreeUser_WithValidData_CreatesItemWithCorrectEndDate()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            userService.GetSubscriptionTypeAsync(Arg.Any<string>()).Returns(Task.FromResult("Free"));
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "Porsche Gt3 Rs",
                Description = "Flat v6",
                StartingPrice = 225000,
                StartDateTime = DateTime.UtcNow.AddSeconds(1),
                CategoryId = 4
            };
            var userId = "AZERTY123";

            // Act
            await service.CreateAuctionItemAsync(newItem, userId);

            // Assert
            await repository.Received(1).AddAsync(Arg.Is<AuctionItem>(ai =>
                ai.Name == newItem.Name &&
                ai.Description == newItem.Description &&
                ai.StartingPrice == newItem.StartingPrice &&
                ai.StartDateTime == newItem.StartDateTime &&
                ai.EndDateTime == newItem.StartDateTime.AddDays(3) && // Correct end date voor Free users
                ai.CategoryId == newItem.CategoryId &&
                ai.SellerId == userId
            ));
        }


        // *************** Correcte aanmaak voor een Gold user met een geldig eindtijdstip ***************
        [Fact]
        public async Task CreateAuctionItemAsync_ForGoldUser_WithValidEndDate_Succeeds()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            userService.GetSubscriptionTypeAsync(Arg.Any<string>()).Returns(Task.FromResult("Gold"));
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "Bentley Flying spur",
                Description = "Edition one W12",
                StartingPrice = 350000,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(12),
                CategoryId = 4
            };
            var userId = "UserTest";

            // Act
            await service.CreateAuctionItemAsync(newItem, userId);

            // Assert
            await repository.Received(1).AddAsync(Arg.Is<AuctionItem>(ai =>
                ai.Name == newItem.Name &&
                ai.Description == newItem.Description &&
                ai.StartingPrice == newItem.StartingPrice &&
                ai.StartDateTime == newItem.StartDateTime &&
                (ai.EndDateTime >= newItem.StartDateTime.AddHours(12)) && // Check voor valid enddate voor Gold users
                ai.CategoryId == newItem.CategoryId &&
                ai.SellerId == userId
            ));
        }


        // *************** Ongeldige einddatum voor een Gold user ***************
        [Fact]
        public async Task CreateAuctionItemAsync_ForGoldUser_WithInvalidEndDate_ThrowsCustomException()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            userService.GetSubscriptionTypeAsync(Arg.Any<string>()).Returns(Task.FromResult("Gold"));
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "Mercedes Maybach s680",
                Description = "Virgil Edition v12",
                StartingPrice = 4000000,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(11), // Intentionally invalid for Gold users
                CategoryId = 4
            };
            var userId = "goldUser";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(
                async () => await service.CreateAuctionItemAsync(newItem, userId));

            exception.Message.Should().Be("Eindtijdstip moet minimaal 12 uur na het starttijdstip liggen.");
        }


        // *************** Ongeldige einddatum voor een Platinum user ***************
        [Fact]
        public async Task CreateAuctionItemAsync_ForPlatinumUser_WithInvalidEndDate_ThrowsCustomException()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            userService.GetSubscriptionTypeAsync(Arg.Any<string>()).Returns(Task.FromResult("Platinum"));
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "BMW M8",
                Description = "Coupe",
                StartingPrice = 120000,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(11), // Opzettelijk ongeldig voor Platinum user
                CategoryId = 4
            };
            var userId = "platinumUser123";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(
                async () => await service.CreateAuctionItemAsync(newItem, userId));

            exception.Message.Should().Be("Eindtijdstip moet minimaal 12 uur na het starttijdstip liggen.");
        }


        // *************** Fout bij annulering door een andere user ***************
        [Fact]
        public async Task CancelAuctionItemAsync_WhenCalledByNonOwner_ThrowsCustomException()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(auctionItemRepository, userService);

            var userId = "user123";
            var anotherUserId = "user456";
            var itemId = 1;

            // Stel je voor dat het item toebehoort aan een andere gebruiker
            auctionItemRepository.GetByIdAsync(itemId)
                .Returns(Task.FromResult(new AuctionItem { SellerId = anotherUserId }));

            // Act
            Func<Task> action = async () => await service.CancelAuctionItemAsync(itemId, userId);

            // Assert
            await action.Should().ThrowAsync<CustomException>()
                .WithMessage("Alleen de verkoper kan de verkoop annuleren.");
        }


        // *************** Verkochte items ophalen voor een specifieke gebruiker ***************
        [Fact]
        public async Task GetSoldItemsAsync_OnlyReturnsItemsSoldToSpecificUser()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(auctionItemRepository, userService);

            var userId = "user123";
            var soldItemsToUser = new List<AuctionItem>
            {
                 new AuctionItem { Id = 1, SellerId = userId, Status = AuctionItemStatus.Paid },
                 new AuctionItem { Id = 2, SellerId = userId, Status = AuctionItemStatus.Paid }
            };

            var otherItems = new List<AuctionItem>
            {
                new AuctionItem { Id = 3, SellerId = "otherUser", Status = AuctionItemStatus.Paid },
                new AuctionItem { Id = 4, SellerId = "anotherUser", Status = AuctionItemStatus.Paid }
            };

            auctionItemRepository.GetSoldItemsByUserIdAsync(userId)
                .Returns(Task.FromResult(soldItemsToUser.AsEnumerable()));

            // Act
            var result = await service.GetSoldItemsAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(soldItemsToUser);
        }

        // *************** het Falen van Item Creatie Bij Ontbrekende Naam ***************
        [Fact]
        public async Task CreateAuctionItemAsync_WithMissingName_ThrowsCustomException()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                // Naam is niet ingesteld
                Description = "Grand Coupe",
                StartingPrice = 100000,
                StartDateTime = DateTime.UtcNow,
                CategoryId = 4
            };
            var userId = "TestUser123";

            // Act & Assert
            await Assert.ThrowsAsync<CustomException>(
                async () => await service.CreateAuctionItemAsync(newItem, userId));
        }


        // *************** het Succesvol Annuleren van een item ***************
        [Fact]
        public async Task CancelAuctionItemAsync_WhenCalledByOwner_Succeeds()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(auctionItemRepository, userService);

            var userId = "TestUser123";
            var itemId = 1;
            auctionItemRepository.GetByIdAsync(itemId)
                .Returns(Task.FromResult(new AuctionItem { SellerId = userId }));

            // Act
            Func<Task> action = async () => await service.CancelAuctionItemAsync(itemId, userId);

            // Assert
            await action.Should().NotThrowAsync<CustomException>();
        }

        // ***************  het Controleren van Incorrecte Starttijd ***************
        [Fact]
        public async Task CreateAuctionItemAsync_WithInvalidStartTime_ThrowsCustomException()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            userService.GetSubscriptionTypeAsync(Arg.Any<string>()).Returns(Task.FromResult("Free"));
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "Aston Marton DBS",
                Description = "Superleggera v12",
                StartingPrice = 300000,
                StartDateTime = DateTime.UtcNow.AddDays(-1), // Incorrecte starttijd
                CategoryId = 4
            };
            var userId = "TestUser123";

            // Act & Assert
            await Assert.ThrowsAsync<CustomException>(
                async () => await service.CreateAuctionItemAsync(newItem, userId));
        }

        // *************** Succesvol bod plaatsen op een geldig veilingitem ***************
        [Fact]
        public async Task PlaceBidAsync_WithLessThanFivePercentIncrease_ThrowsCustomException()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(auctionItemRepository, userService);

            var auctionItemId = 1;
            var bidderId = "Testbidder123";
            var currentHighestBidAmount = 100;
            var newBidAmount = 104; // Minder dan 5% verhoging

            var auctionItem = new AuctionItem
            {
                Id = auctionItemId,
                SellerId = "seller456",
                Status = AuctionItemStatus.Initial,
                StartDateTime = DateTime.UtcNow.AddDays(-1),
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Bids = new List<Bid>() { new Bid { Amount = currentHighestBidAmount, BidderId = "otherBidder" } }
            };

            auctionItemRepository.GetByIdAsync(auctionItemId).Returns(Task.FromResult(auctionItem));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(
                async () => await service.PlaceBidAsync(auctionItemId, bidderId, newBidAmount));

            exception.Message.Should().Be("Het nieuwe bod moet minimaal 5% hoger zijn dan het huidige hoogste bod.");
        }


        // ***************  het Verifiëren van de Startprijs ***************
        [Fact]
        public async Task CreateAuctionItemAsync_WithInvalidStartingPrice_ThrowsCustomException()
        {
            // Arrange
            var repository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(repository, userService);

            var newItem = new CreateAuctionItemDto
            {
                Name = "ItemNaam",
                Description = "Beschrijving",
                StartingPrice = -100, // Ongeldige startprijs
                StartDateTime = DateTime.UtcNow,
                CategoryId = 1
            };
            var userId = "TestUser123";

            // Act & Assert
            await Assert.ThrowsAsync<CustomException>(
                async () => await service.CreateAuctionItemAsync(newItem, userId));
        }



        // Bieden op items
        // *************** Succesvol bod plaatsen op een geldig veilingitem ***************
        [Fact]
        public async Task PlaceBidAsync_OnValidItem_PlacesBidSuccessfully()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var service = new AuctionItemService(auctionItemRepository, null);

            var auctionItemId = 1;
            var bidderId = "bidder123";
            var bidAmount = 200;
            var auctionItem = new AuctionItem
            {
                Id = auctionItemId,
                SellerId = "seller456",
                Status = AuctionItemStatus.Initial,
                StartDateTime = DateTime.UtcNow.AddDays(-1),
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Bids = new List<Bid>() { new Bid { Amount = 100, BidderId = "otherBidder" } }
            };

            auctionItemRepository.GetByIdAsync(auctionItemId).Returns(Task.FromResult(auctionItem));
            auctionItemRepository.PlaceBidAsync(auctionItemId, bidderId, bidAmount).Returns(Task.FromResult(new Bid()));

            // Act
            await service.PlaceBidAsync(auctionItemId, bidderId, bidAmount);

            // Assert
            await auctionItemRepository.Received(1).PlaceBidAsync(auctionItemId, bidderId, bidAmount);
        }

        // ***************  testen of een gebruiker niet op zijn eigen items kan bieden ***************
        [Fact]
        public async Task PlaceBidAsync_UserCannotBidOnOwnItem_ThrowsCustomException()
        {
            // Arrange
            var auctionItemRepository = Substitute.For<IAuctionItemRepository>();
            var userService = Substitute.For<IUserService>();
            var service = new AuctionItemService(auctionItemRepository, userService);

            var userId = "user123";
            var itemId = 1;

            // Stel je voor dat het item toebehoort aan dezelfde gebruiker
            auctionItemRepository.GetByIdAsync(itemId)
                .Returns(Task.FromResult(new AuctionItem { SellerId = userId }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(
                async () => await service.PlaceBidAsync(itemId, userId, 100.00m));

            exception.Message.Should().Be("Je kunt niet bieden op je eigen veilingitem.");
        }




    }
}
