using AStore.Models;
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
    public partial class AddProductWindow : Window
    {

        private ProductService _productService;

        public delegate void ProductAddedEventHandler(object sender, EventArgs e);
        public event ProductAddedEventHandler ProductAdded;

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
            // Проверка на пустые или некорректные значения
            if (string.IsNullOrWhiteSpace(ProductName.Text) ||
                string.IsNullOrWhiteSpace(ProductDescription.Text) ||
                string.IsNullOrWhiteSpace(ProductPrice.Text) ||
                ProductCategory.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены корректно.");
                return;
            }

            // Попытка преобразовать цену из текста
            if (!decimal.TryParse(ProductPrice.Text, out decimal price))
            {
                MessageBox.Show("Некорректно указана цена.");
                return;
            }

            var selectedCategory = (Category)ProductCategory.SelectedItem;

            // Создание экземпляра нового продукта
            var newProduct = new Product
            {
                Name = ProductName.Text,
                Description = ProductDescription.Text,
                Price = price,
                CategoryId = selectedCategory.CategoryId,
            };

            // Добавление продукта в базу данных с помощью ProductService
            var addedProduct = _productService.AddProduct(newProduct);

            // Проверка добавился ли продукт в базу данных
            if (addedProduct != null)
            {
                MessageBox.Show("Продукт добавлен успешно.");

                ProductAdded?.Invoke(this, EventArgs.Empty);
                this.Close(); // Закрытие окна после добавления
            }
            else
            {
                MessageBox.Show("Произошла ошибка при добавлении продукта.");
            }
        }
    }
}
