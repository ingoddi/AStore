using AStore.Models;
using AStore.Services;
using AStore.Services.TablesServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AStore.Screens
{
    public partial class CartScreen : Window
    {
        private CartService _cartService;
        private int _userId;

        public CartScreen(int userId)
        {
            InitializeComponent();
            _cartService = new CartService(new DatabaseManager());
            _userId = userId; 

            LoadCartItems();
        }

        public void UpdateCartItemQuantity(int userId, int productId, int quantity)
        {
       
        }

        private void LoadCartItems()
        {
            var cartItems = _cartService.GetCartItems(_userId);
            CartItemsListView.ItemsSource = cartItems;

            CartItemsListView.Items.Refresh();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as CartItem;
            _cartService.RemoveFromCart(_userId, item.ProductId);
            LoadCartItems();
        }

        private void UpdateQuantity_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
          _cartService.CreateOrder(_userId);
          var lastOrder = _cartService.GetLastOrderForUser(_userId);
           _cartService.GenerateOrderPdf(lastOrder);
          MessageBox.Show("Заказ успешно оформлен и сохранен в PDF!");
        }
    }
}
