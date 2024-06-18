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
using QRCoder;
using iTextSharp.text;
using System.Drawing;

namespace AStore.Services.TablesServices
{
    public class CartService
    {

        private DatabaseManager _dbManager;
        private static Dictionary<int, List<CartItem>> _userCarts = new Dictionary<int, List<CartItem>>();
        private static Dictionary<int, List<Order>> _userOrders = new Dictionary<int, List<Order>>();

        public CartService(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public List<CartItem> GetCartItems(int userId)
        {
            return _userCarts.ContainsKey(userId) ? _userCarts[userId] : new List<CartItem>();
        }

        public void AddToCart(int userId, int productId, int quantity)
        {
            if (!_userCarts.ContainsKey(userId))
            {
                _userCarts[userId] = new List<CartItem>();
            }

            var cartItem = _userCarts[userId].FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                var product = _dbManager.Products.SingleOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    _userCarts[userId].Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Product = product
                    });
                }
            }
        }

        public void RemoveFromCart(int userId, int productId)
        {
            if (_userCarts.ContainsKey(userId))
            {
                var cartItem = _userCarts[userId].FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    _userCarts[userId].Remove(cartItem);
                }
            }
        }

        public void CreateOrder(int userId)
        {
            if (!_userCarts.ContainsKey(userId) || !_userCarts[userId].Any())
            {
                Console.WriteLine("Cart is empty. Order creation aborted.");
                return;
            }

            var cartItems = _userCarts[userId];
            var totalPrice = cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now.ToString(),
                TotalPrice = Convert.ToInt32(totalPrice),
                StatusId = 1,
                OrderItems = cartItems.Select(ci => new OrderItems
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product?.Price ?? 0
                }).ToList()
            };

            if (!_userOrders.ContainsKey(userId))
            {
                _userOrders[userId] = new List<Order>();
            }
            _userOrders[userId].Add(order);

            GenerateOrderQRCode(order);
            GenerateOrderPdf(order);

            _userCarts[userId].Clear(); 

            Console.WriteLine("Order created successfully.");
        }

        public Order GetLastOrderForUser(int userId)
        {
            if (_userOrders.ContainsKey(userId) && _userOrders[userId].Any())
            {
                return _userOrders[userId].LastOrDefault();
            }

            return null; 
        }

        public decimal GetTotalCartPrice(int userId)
        {
            if (!_userCarts.ContainsKey(userId) || !_userCarts[userId].Any())
            {
                return 0; 
            }

            decimal total = 0;
            foreach (var item in _userCarts[userId])
            {
                var productPrice = _dbManager.Products.SingleOrDefault(p => p.ProductId == item.ProductId)?.Price ?? 0;
                total += item.Quantity * productPrice;
            }

            return total;
        }

        public List<Order> GetOrders(int userId)
        {
            if (_userOrders.ContainsKey(userId))
            {
                return _userOrders[userId];
            }
            return new List<Order>();
        }

        public void GenerateOrderPdf(Order order)
        {
            if (order == null || order.OrderItems == null)
            {
                throw new ArgumentNullException(nameof(order), "Order or Order Items is null.");
            }

            string directoryPath = "result";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = $"Order_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(directoryPath, fileName);

            using (PdfDocument document = new PdfDocument())
            {
                PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                document.Add(new Paragraph("Заказ №" + order.OrderId));
                document.Add(new Paragraph($"Дата создания: {order.CreatedAt}"));
                document.Add(new Paragraph("Товары:"));

                foreach (var item in order.OrderItems)
                {
                    var productName = item.Product?.Name ?? "Название неизвестно";
                    document.Add(new Paragraph($"{productName}, Количество: {item.Quantity}, Цена: {item.Price}"));
                }

                document.Add(new Paragraph($"Общая цена: {order.TotalPrice}"));
            }
        }

        public void GenerateOrderQRCode(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order is null.");
            }

            string directoryPath = "result";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string orderUrl = $"http://example.com/order/{order.OrderId}";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(orderUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            {
                string filePath = Path.Combine(directoryPath, $"Order_{order.OrderId}_QR.png");
                qrCodeImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"QR-код для заказа {order.OrderId} сохранён в файл: {filePath}");
            }
        }
    }
}
