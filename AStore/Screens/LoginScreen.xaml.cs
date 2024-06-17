using AStore.Screens;
using AStore.Services;
using System.Text;
using AStore.Services.TablesServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AStore
{
    public partial class LoginScreen : Window
    {

        private UserService _userService;
        private ProductService productService = new ProductService(new DatabaseManager());
        private CartService cartService = new CartService(new DatabaseManager());
        private Boolean isManager = false;

        public LoginScreen()
        {
            InitializeComponent();
            _userService = new UserService(new DatabaseManager());
        }

        // Переход к регистрации
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationScreen();
            registrationWindow.Show();
            this.Close();
        }

        // Вход
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("^.{8,}$");
            if (!regex.IsMatch(password.Password))
            {
                MessageBox.Show("Неверный формат пароля. Минимум восемь символов");
                return;
            }

            var user = _userService.GetUser(username.Text, password.Password);

            if (user != null)
            {
                if (loginAsManager.IsChecked == true)
                {
                    var manager = _userService.GetManager(user.UserId);

                    if (manager != null)
                    {
                        MessageBox.Show("Успешный вход для менеджера.");
                        isManager = true;
                        var userWindow = new Catalog(user.UserId, productService, cartService, isManager);
                        userWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Данная учетная запись не имеет прав менеджера.");
                    }
                }
                else
                {
                    MessageBox.Show("Успешный вход для пользователя.");
                    var userWindow = new Catalog(user.UserId, productService, cartService, isManager);
                    userWindow.Show();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Пользователь не найден.");
            }
        }
    }
}