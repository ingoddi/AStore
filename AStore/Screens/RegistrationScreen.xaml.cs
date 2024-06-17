using AStore.Services;
using AStore.Services.TablesServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class RegistrationScreen : Window
    {
        private UserService _userService;

        private string ManagerSecretKey = "qweeqw321";

        public RegistrationScreen()
        {
            InitializeComponent();
            this.managerSecretKey.Visibility = Visibility.Hidden;
            _userService = new UserService(new DatabaseManager());
        }

        //Регистрация
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Password) || string.IsNullOrWhiteSpace(confirmPassword.Password))
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return;
            }

            if (_userService.UserExists(username.Text))
            {
                MessageBox.Show("Пользователь с таким именем уже существует. Пожалуйста, выберите другое имя.");
                return;
            }

            Regex regex = new Regex("^.{8,}$");

            if (!regex.IsMatch(password.Password))
            {
                MessageBox.Show("Неверный формат пароля. Минимум восемь символов");
                return;
            }

            if (password.Password != confirmPassword.Password)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            _userService.CreateUser(username.Text, password.Password);

            if (loginAsManager.IsChecked == true)
            {
                if (managerSecretKey.Text == ManagerSecretKey)
                {
                    _userService.CreateManager(username.Text, password.Password, 1);
                    MessageBox.Show("Вы успешно зарегестрировались как менеджер");
                }
                else
                {
                    MessageBox.Show("Неверный секретный ключ менеджера.");
                }

            }

            MessageBox.Show("Вы успешно зарегестрировались как пользователь");

        }

        //Переход к входу
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var userWindow = new LoginScreen();
            userWindow.Show();
            this.Close();
        }

        private void loginAsManager_Checked(object sender, RoutedEventArgs e)
        {
            managerSecretKey.Visibility = Visibility.Visible;
        }

        private void loginAsManager_Unchecked(object sender, RoutedEventArgs e)
        {
            managerSecretKey.Visibility = Visibility.Hidden;
        }
    }
}
