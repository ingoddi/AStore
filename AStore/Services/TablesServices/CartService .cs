using AStore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var cart = _dbManager.Carts
                             .Include(c => c.CartItems)
                             .SingleOrDefault(c => c.User.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _dbManager.Carts.Add(cart);
                _dbManager.SaveChanges();
            }

            var cartItem = cart.CartItems
                               .SingleOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            _dbManager.SaveChanges();
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



    }
}
