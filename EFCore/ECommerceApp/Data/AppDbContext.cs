using ECommerceApp.Enums;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace ECommerceApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DateTime seedDate = new(2025, 01, 01, 10, 00, 00);

            // Order Status
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { OrderStatusId = 1, Name = OrderStatusEnum.Pending.ToString(), Description = "Awaiting confirmation", CreatedAt = seedDate },
                new OrderStatus { OrderStatusId = 2, Name = OrderStatusEnum.Confirmed.ToString(), Description = "Confirmed by admin", CreatedAt = seedDate },
                new OrderStatus { OrderStatusId = 3, Name = OrderStatusEnum.Shipped.ToString(), Description = "Dispatched to courier", CreatedAt = seedDate },
                new OrderStatus { OrderStatusId = 4, Name = OrderStatusEnum.Delivered.ToString(), Description = "Delivered successfully", CreatedAt = seedDate },
                new OrderStatus { OrderStatusId = 5, Name = OrderStatusEnum.Cancelled.ToString(), Description = "Cancelled by user/system", CreatedAt = seedDate }
            );

            // Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "Pranaya Rout", Email = "pranaya@example.com", Phone = "9876543210", CreatedAt = seedDate },
                new Customer { CustomerId = 2, Name = "Rakesh Kumar", Email = "rakesh@example.com", Phone = "9876543211", CreatedAt = seedDate }
            );

            // Profiles 
            modelBuilder.Entity<Profile>().HasData(
                new Profile { CustomerId = 1, DisplayName = "Pranaya", Gender = "Male", DateOfBirth = new(1990, 05, 10), CreatedAt = seedDate },
                new Profile { CustomerId = 2, DisplayName = "Rakesh", Gender = "Female", DateOfBirth = new(1992, 08, 22), CreatedAt = seedDate }
            );

            // Addresses
            modelBuilder.Entity<Address>().HasData(
                new Address { AddressId = 1, Line1 = "123 Market Street", Street = "Main Rd", City = "Bhubaneswar", PostalCode = "751001", Country = "India", CustomerId = 1, CreatedAt = seedDate },
                new Address { AddressId = 2, Line1 = "Tech Park Avenue", Street = "Silicon Street", City = "Cuttack", PostalCode = "753001", Country = "India", CustomerId = 1, CreatedAt = seedDate },
                new Address { AddressId = 3, Line1 = "45 Lake View", Street = "West Road", City = "Bhubaneswar", PostalCode = "751010", Country = "India", CustomerId = 2, CreatedAt = seedDate }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Description = "Electronic Devices", CreatedAt = seedDate },
                new Category { CategoryId = 2, Name = "Books", Description = "Educational and Fiction", CreatedAt = seedDate }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Wireless Mouse", Price = 1200, Stock = 50, SKU = "ELEC-MOUSE-001", CategoryId = 1, CreatedAt = seedDate },
                new Product { ProductId = 2, Name = "Bluetooth Headphones", Price = 2500, Stock = 40, SKU = "ELEC-HEAD-002", CategoryId = 1, CreatedAt = seedDate },
                new Product { ProductId = 3, Name = "C# Programming Book", Price = 899, Stock = 100, SKU = "BOOK-CSPROG-003", CategoryId = 2, CreatedAt = seedDate }
            );

            // Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, OrderDate = seedDate, OrderStatusId = 2, TotalAmount = 3700, ShippingAddressId = 1, BillingAddressId = 2, CustomerId = 1, CreatedAt = seedDate },
                new Order { OrderId = 2, OrderDate = seedDate.AddDays(1), OrderStatusId = 3, TotalAmount = 899, ShippingAddressId = 3, BillingAddressId = 3, CustomerId = 2, CreatedAt = seedDate }
            );

            // OrderItems
            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 1200, CreatedAt = seedDate },
                new OrderItem { OrderItemId = 2, OrderId = 1, ProductId = 2, Quantity = 1, UnitPrice = 2500, CreatedAt = seedDate },
                new OrderItem { OrderItemId = 3, OrderId = 2, ProductId = 3, Quantity = 1, UnitPrice = 899, CreatedAt = seedDate }
            );
        }
    }
}
