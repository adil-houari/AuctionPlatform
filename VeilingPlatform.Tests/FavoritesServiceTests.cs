using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeilingPlatform.Entities;

namespace VeilingPlatform.Tests
{

    // Arrange
    // Act
    // Assert
    public class FavoritesServiceTests
    {

        // *************** het succesvol toevoegen van een item aan de favorietenlijst ***************
        [Fact]
        public async Task AddToFavorite_Success_ReturnsFavorite()
        {
            // Arrange
            var repository = Substitute.For<IFavoriteRepository>();
            var service = new FavoriteService(repository);

            var auctionItemId = 1;
            var userId = "user123";
            var favorite = new Favorite { AuctionItemId = auctionItemId, UserId = userId };

            repository.AddToFavorite(auctionItemId, userId).Returns(favorite);

            // Act
            var result = await service.AddToFavorite(auctionItemId, userId);

            // Assert
            result.Should().BeEquivalentTo(favorite);
        }


        // *************** het succesvol verwijderen van een item uit de favorietenlijst ***************
        [Fact]
        public async Task RemoveFromFavorite_Success_ReturnsTrue()
        {
            // Arrange
            var repository = Substitute.For<IFavoriteRepository>();
            var service = new FavoriteService(repository);

            var favoriteId = 1;
            var userId = "user123";

            repository.RemoveFromFavorite(favoriteId, userId).Returns(true);

            // Act
            var result = await service.RemoveFromFavorite(favoriteId, userId);

            // Assert
            result.Should().BeTrue();
        }


        // *************** het ophalen van favoriete items voor een specifieke gebruiker ***************
        [Fact]
        public async Task GetUserFavorites_ReturnsListOfFavorites()
        {
            // Arrange
            var repository = Substitute.For<IFavoriteRepository>();
            var service = new FavoriteService(repository);

            var userId = "user123";
            var favoriteItems = new List<Favorite>
            {
                new Favorite { Id = 1, UserId = userId },
                new Favorite { Id = 2, UserId = userId },
                new Favorite { Id = 3, UserId = userId }
            };

            repository.GetUserFavorites(userId).Returns(favoriteItems);

            // Act
            var result = await service.GetUserFavorites(userId);

            // Assert
            result.Should().BeEquivalentTo(favoriteItems);
        }
    }
}
