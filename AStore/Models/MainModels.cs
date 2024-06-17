using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AStore.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }

    public class Manager
    {
        public int ManagerId { get; set; }
        public int AccessLevel { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public class AccessLevel
    {
        public int AccessLevelId { get; set; }
        public string AccessLevelName { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class OrderItems
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        public int ProductId { get; set; } 
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int TotalPrice { get; set; }
        public int StatusId { get; set; }
        public string CreatedAt { get; set; }
        public int UserId { get; set; } // Внешний ключ для связи с User
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }

    public class OrdersStatuses
    {
        public int StatusId { get; set; }
        public string StatusName {  get; set; }
    }

    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; } 
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
