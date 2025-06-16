using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VeilingPlatform.Entities;

namespace VeilingPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuctionItem>().Property(x => x.StartingPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Bid>().Property(x => x.Amount).HasPrecision(18, 2);

            modelBuilder.Entity<Category>().HasData(
                    new Category { Id = 1, Name = "Kunst" },
                    new Category { Id = 2, Name = "Horloges" },
                    new Category { Id = 3, Name = "Antiek" },
                    new Category { Id = 4, Name = "Voertuigen" },
                    new Category { Id = 5, Name = "Juwelen" },
                    new Category { Id = 6, Name = "Elektronica"},
                    new Category { Id = 7, Name = "Boeken" }
                 );
        }


        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
    }
}
