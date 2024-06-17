using AStore.Models;
using AStore.Screens.Common;
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
    public partial class Catalog : Window
    {
        private ProductService _productService;
        private CartService _cartService;
        private int _userId;
        private Boolean _isManager;

        public Catalog(int userId, ProductService productService, CartService cartService, bool isManager)
        {
            InitializeComponent();

            _userId = userId;
            _productService = productService;
            _cartService = cartService;
            _isManager = isManager;

            LoadProducts();

            if (_isManager)
            {
                ProductGrid.IsReadOnly = false;
            }
            else
            {
                ProductGrid.IsReadOnly = true;
            }
        }
        
        private void LoadProducts()
        {
            var products = _productService.GetAllProducts();
            ProductGrid.ItemsSource = products;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                ProductDTO? product = button.DataContext as ProductDTO;

                if (product != null)
                {
                    _cartService.AddToCart(_userId, product.ProductId, 1);
                    MessageBox.Show($"Продукт {product.Name} добавлен в корзину.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void ProductGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!_isManager || e.EditAction != DataGridEditAction.Commit) return;

            var rowView = e.Row.Item as Product;
            if (rowView != null)
            {
                var productId = rowView.ProductId;
                var product = _productService.GetProductById(productId);

                if (product != null)
                {
                    if (e.Column.Header.ToString() == "Name")
                    {
                        var textBox = e.EditingElement as TextBox;
                        product.Name = textBox.Text;
                    }

                    _productService.UpdateProduct(product);
                }
            }
        }

        private void ProductGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartScreen(_userId);
            cartWindow.Show();
            this.Close();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddProductWindow(_productService);
            addProductWindow.ProductAdded += AddProductWindow_ProductAdded;
            addProductWindow.ShowDialog();
        }

        private void AddProductWindow_ProductAdded(object sender, EventArgs e)
        {
            LoadProducts();
        }
    }
}
