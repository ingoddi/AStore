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

namespace AStore.Screens.Common
{
    private ProductService productService;

    public partial class AddProductWindow : Window
    {
        public AddProductWindow(ProductService productService)
        {
            InitializeComponent();
            _productService = productService;

            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = _productService.GetAllCategories();
            ProductCategory.ItemsSource = categories;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
