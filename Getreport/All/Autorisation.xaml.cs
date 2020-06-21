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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using Getreport.Admin;
using System.IO;

namespace Getreport
{
    /// <summary>
    /// Логика взаимодействия для Autorisation.xaml
    /// </summary>
    public partial class Autorisation : Page
    {

        public SqlConnection sqlConnection;
        public Autorisation()
        {
            InitializeComponent();

            connectDB();
        }

        private async void connectDB()
        {

            //string pathBD = Environment.CurrentDirectory + @"\GetreportDB.mdf";
            string connectionStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kosty\source\repos\Getreport\Getreport\GetreportDB.mdf;Integrated Security=True";

            sqlConnection = new SqlConnection(connectionStr);

            if (sqlConnection.State == ConnectionState.Closed)
                await sqlConnection.OpenAsync();
        }

        private async void checkLoginAndPassword(string loginStr, string passStr)
        {
            bool pass = false;

            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand("SELECT * FROM Пользователи", sqlConnection);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    if(Convert.ToString(sqlReader["Логин"]) == loginStr && Convert.ToString(sqlReader["Пароль"]) == passStr)
                    {
                        pass = true;
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
                App.Current.MainWindow.Content = new MainMenuAdmin();
            }
            else
            {
                errorMessage.Visibility = Visibility.Visible;
            }
            
        }

        private void checkEmptyBox()
        {
            if (passBox.Password != "" && loginBox.Text != "")
            {
                login.IsEnabled = true;
            }
            else
            {
                login.IsEnabled = false;
            }
        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();

            App.Current.MainWindow.Content = new Registration();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            errorMessage.Visibility = Visibility.Collapsed;
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
            checkLoginAndPassword(loginBox.Text, passBox.Password);
        }

        private void loginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyBox();
        }

        private void passBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkEmptyBox();
        }

    }
}
