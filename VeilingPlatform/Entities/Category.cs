﻿namespace VeilingPlatform.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<AuctionItem> AuctionItems { get; set; }
    }

}
