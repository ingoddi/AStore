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

        public async Task AddToCart(int userId, int productId, int quantity)
        {
            // Поиск существующей корзины пользователя или создание новой
            var cart = await _dbManager.Carts.Include(c => c.CartItems)
                                        .SingleOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _dbManager.Carts.Add(cart);
                await _dbManager.SaveChangesAsync();
            }

            // Поиск существующего товара в корзине
            var cartItem = cart.CartItems.SingleOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                // Если товар уже есть, увеличить его количество
                cartItem.Quantity += quantity;
            }
            else
            {
                // Иначе добавить новый товар в корзину
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.CartId
                };
                cart.CartItems.Add(cartItem);
            }

            await _dbManager.SaveChangesAsync();
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


        public async Task CreateOrder(int userId)
        {
            var cart = await _dbManager.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                              .SingleOrDefaultAsync(c => c.UserId == userId);
            if (cart != null && cart.CartItems.Count > 0)
            {
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

                await _dbManager.Orders.AddAsync(order);
                await _dbManager.SaveChangesAsync();

                _dbManager.CartItems.RemoveRange(cart.CartItems);
                await _dbManager.SaveChangesAsync();
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
