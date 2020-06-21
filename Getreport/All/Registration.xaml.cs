using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using Getreport.Admin;

namespace Getreport
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Page
    {
        SqlConnection sqlConnection;

        public Registration()
        {
            InitializeComponent();

            connectDB();
        }

        private async void connectDB()
        {
            string connectionStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kosty\source\repos\Getreport\Getreport\GetreportDB.mdf;Integrated Security=True";

            sqlConnection = new SqlConnection(connectionStr);

            if (sqlConnection.State == ConnectionState.Closed)
                await sqlConnection.OpenAsync();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Content = new Autorisation();
        }

        private void checkEmptyBox()
        {
            if (loginBox.Text != "" && passBox.Password != "" && doublePassBox.Password != "" &&
                firstBox.Text != "" && secondBox.Text != "" && thirdBox.Text != "" && phoneBox.Text != "")
            {
                login.IsEnabled = true;
            }
            else
            {
                login.IsEnabled = false;
            }
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            errorSameLogin.Visibility = Visibility.Collapsed;

            errorIncorrectSymbols.Visibility = Visibility.Collapsed;

            errorPhone.Visibility = Visibility.Collapsed;

            errorPasswords.Visibility = Visibility.Collapsed;

            errorPasswords2.Visibility = Visibility.Collapsed;

            errorLogin.Visibility = Visibility.Collapsed;

            errorFIO.Visibility = Visibility.Collapsed;

            int countPass = 0;

            login.IsEnabled = true;

            Regex regex = new Regex(@"\n[A-zА-я0-9ё]+\n");

            string str = $"\n{loginBox.Text}\n\n{firstBox.Text}\n\n{secondBox.Text}\n\n{thirdBox.Text}\n\n{passBox.Password}\n\n{doublePassBox.Password}\n";

            if (regex.Matches(str).Count == 6)
            {
                countPass++;
            }
            else
            {
                errorIncorrectSymbols.Visibility = Visibility.Visible;
            }

            regex = new Regex(@"^\+[0-9][0-9][0-9] [0-9][0-9][0-9] [0-9][0-9][0-9]\-[0-9][0-9]\-[0-9][0-9]$");

            if (regex.Matches(phoneBox.Text).Count == 1)
            {
                countPass++;
            }
            else
            {
                errorPhone.Visibility = Visibility.Visible;
            }

            if (doublePassBox.Password == passBox.Password)
            {
                countPass++;
            }
            else
            {
                errorPasswords.Visibility = Visibility.Visible;
            }

            regex = new Regex(@"[А-я]+");
            Regex regex2 = new Regex(@"[0-9]+");

            if (regex.Matches(passBox.Password).Count == 0 && passBox.Password.Length >= 8 && regex2.Matches(passBox.Password).Count >= 1)
            {
                countPass++;
            }
            else
            {
                errorPasswords2.Visibility = Visibility.Visible;
            }

            regex = new Regex(@"[А-я]+");

            if (regex.Matches(loginBox.Text).Count == 0)
            {
                countPass++;
            }
            else
            {
                errorLogin.Visibility = Visibility.Visible;
            }

            regex = new Regex(@"[A-z]+");

            if (regex.Matches(firstBox.Text).Count == 0 && regex2.Matches(firstBox.Text).Count == 0 &&
                regex.Matches(secondBox.Text).Count == 0 && regex2.Matches(secondBox.Text).Count == 0 &&
                regex.Matches(thirdBox.Text).Count == 0 && regex2.Matches(thirdBox.Text).Count == 0)
            {
                countPass++;
            }
            else
            {
                errorFIO.Visibility = Visibility.Visible;
            }

            if(countPass == 6)
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                    createUser();
            }
            // App.Current.MainWindow.Content = new MainMenu();
        }

        private async void createUser()
        {
            bool pass = true;

            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand($"SELECT * FROM Пользователи", sqlConnection);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    if (Convert.ToString(sqlReader["Логин"]) == loginBox.Text)
                    {
                        pass = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (sqlReader != null)
                sqlReader.Close();

            if (pass)
            {
                command = new SqlCommand($"INSERT Пользователи(Имя,Фамилия,Отчество,Логин,Пароль,Телефон,Должность) VALUES(N'{firstBox.Text}', N'{secondBox.Text}', N'{thirdBox.Text}', '{loginBox.Text}', '{passBox.Password}', '{phoneBox.Text}', N'пользователь')", sqlConnection);
                await command.ExecuteNonQueryAsync();
                App.Current.MainWindow.Content = new MainMenuAdmin();
            }
            else
            {
                errorSameLogin.Visibility = Visibility.Visible;
            }

        }

        private void loginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }

        private void passBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkEmptyBox();
        }

        private void doublePassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkEmptyBox();
        }

        private void firstBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }

        private void secondBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }

        private void thirdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }

        private void phoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }
    }
}
