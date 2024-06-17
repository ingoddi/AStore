using AStore.Models;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfDocument = iTextSharp.text.Document;
using iTextSharp.text;

namespace AStore.Services.TablesServices
{
    public class CartService
    {

        private DatabaseManager _dbManager;

        public CartService(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public List<CartItem> GetCartItems(int userId)
        {
            return _dbManager.CartItems.Where(ci => ci.Cart.User.UserId == userId)
                                       .Include(ci => ci.Product)
                                       .ToList();
        }

        public void AddToCart(int userId, int productId, int quantity)
        {
            Console.WriteLine($"AddToCart start: userId={userId}, productId={productId}, quantity={quantity}");
            try
            {
                var cart = _dbManager.Carts.Include(c => c.CartItems)
                                           .SingleOrDefault(c => c.UserId == userId); // Изменено с SingleOrDefaultAsync на SingleOrDefault

                if (cart == null)
                {
                    Console.WriteLine("Cart not found, creating new one.");
                    cart = new Cart { UserId = userId };
                    _dbManager.Carts.Add(cart);
                    _dbManager.SaveChanges(); // Изменено с SaveChangesAsync на SaveChanges
                }

                var cartItem = cart.CartItems.SingleOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    Console.WriteLine("Product already in cart, updating quantity.");
                    cartItem.Quantity += quantity;
                }
                else
                {
                    Console.WriteLine("Adding new product to cart.");
                    cartItem = new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        CartId = cart.CartId
                    };
                    cart.CartItems.Add(cartItem);
                }

                _dbManager.SaveChanges(); // Изменено с SaveChangesAsync на SaveChanges
                Console.WriteLine("AddToCart completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToCart error: {ex.Message}");
            }
        }

        public void RemoveFromCart(int userId, int productId)
        {
            var cartItem = _dbManager.CartItems
                                     .Include(ci => ci.Cart)
                                     .FirstOrDefault(ci => ci.Cart.User.UserId == userId && ci.ProductId == productId);

            if (cartItem != null)
            {
                _dbManager.CartItems.Remove(cartItem);
                _dbManager.SaveChanges();
            }
        }


        public void CreateOrder(int userId)
        {
            Console.WriteLine($"CreateOrder start: userId={userId}");
            try
            {
                var cart = _dbManager.Carts.Include(c => c.CartItems)
                                           .ThenInclude(ci => ci.Product)
                                           .SingleOrDefault(c => c.UserId == userId); 

                if (cart == null || cart.CartItems.Count == 0)
                {
                    Console.WriteLine("Cart is empty or not found. Order creation aborted.");
                    return;
                }

                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now.ToString(),
                    TotalPrice = Convert.ToInt32(cart.CartItems.Sum(item => item.Quantity * item.Product.Price ?? 0m)),
                    StatusId = 1
                };

                foreach (var item in cart.CartItems)
                {
                    order.OrderItems.Add(new OrderItems
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price ?? 0
                    });
                }

                Console.WriteLine($"Creating order for userId={userId}.");
                _dbManager.Orders.Add(order); 
                _dbManager.SaveChanges(); 

                _dbManager.CartItems.RemoveRange(cart.CartItems);
                _dbManager.SaveChanges(); 
                Console.WriteLine("Order created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateOrder error: {ex.Message}");
            }
        }

        public Order GetLastOrderForUser(int userId)
        {
            return _dbManager.Orders
                             .Where(o => o.UserId == userId)
                             .OrderByDescending(o => o.CreatedAt) 
                             .FirstOrDefault();
        }

        public void GenerateOrderPdf(Order order)
        {
            if (order == null || order.OrderItems == null)
            {
                throw new ArgumentNullException(nameof(order), "Order or OrderItems is null.");
            }

            string fileName = $"Order_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            PdfDocument document = new PdfDocument();
            try
            {
                PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                document.Add(new Paragraph("Заказ №" + order.OrderId));
                document.Add(new Paragraph($"Дата создания: {order.CreatedAt}"));
                document.Add(new Paragraph("Товары:"));

                foreach (var item in order.OrderItems)
                {
                    if (item.Product == null)
                    {
                        throw new ArgumentException("Product is null in one of the OrderItems", nameof(order));
                    }
                    document.Add(new Paragraph($"{item.Product.Name}, Количество: {item.Quantity}, Цена: {item.Price}"));
                }

                document.Add(new Paragraph($"Общая цена: {order.TotalPrice}"));

            }
            finally
            {
                if (document != null) document.Close();
            }
        }
    }
}
