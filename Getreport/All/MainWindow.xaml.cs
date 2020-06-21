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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //File.Create("Клиент.txt");
            //Content = new Autorisation();
            Content = new MainMenuAdmin();
            //Content = new ClientPageAdmin();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //const string dir = @"..\..\img\imgClients";

            //foreach (string file in Directory.GetFiles(dir))
            //    File.Delete(file);

            string connectionStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kosty\source\repos\Getreport\Getreport\GetreportDB.mdf;Integrated Security=True";

            SqlConnection sqlConnection = new SqlConnection(connectionStr);

            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }
    }
}
