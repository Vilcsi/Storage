using System.Data.Entity;

namespace StoreHouse.Models
{
    public class StorageContext : DbContext
    {
        public StorageContext() : base("StorageContext") { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; } 
    }
}