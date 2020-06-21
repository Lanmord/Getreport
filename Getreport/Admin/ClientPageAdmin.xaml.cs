using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

namespace Getreport.Admin
{
    /// <summary>
    /// Логика взаимодействия для ClientPageAdmin.xaml
    /// </summary>
    public partial class ClientPageAdmin : Page
    {
        Person client = new Person();
        List<Report> reports = new List<Report>();
        List<Accrual> accruals = new List<Accrual>();
        List<double> tariffs = new List<double>();

        SqlConnection sqlConnection;

        int monthNum = 1;
        int yearNum = 2020;
        string imagePath = "";
        public ClientPageAdmin()
        {
            InitializeComponent();

            //List<int> itemYears = new List<int>();
            //int year = 1995;
            //DateTime now = DateTime.Now;

            //for (int i = year; i <= now.Year; i++)
            //{
            //    itemYears.Add(year);
            //    year++;
            //}
            //comboboxDate.ItemsSource = itemYears;
            //comboboxDate.SelectedItem = now.Year;

            File.Delete(@"..\..\img\imgClients\ClientPhoto.jpg");

            connectDB();

            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                loadInfo();

            image.Source = BitmapFromUri(new Uri(System.IO.Path.GetFullPath(@"..\..\img\imgClients\ClientPhoto.jpg")));

            firstBlock.Text = client.FirstName;
            secondBlock.Text = client.SecondName;
            thirdBlock.Text = client.ThirdName;
            addressBlock.Text = client.Address;
            phoneBlock.Text = client.PhoneNumber;
            personalAccountBlock.Text = client.PersonalAccount;
            personaIdBlock.Text = client.IdNumber;

        }
        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
        private void connectDB()
        {

            string connectionStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kosty\source\repos\Getreport\Getreport\GetreportDB.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionStr);

            if (sqlConnection.State == ConnectionState.Closed)
                sqlConnection.Open();

            getTariff();

        }
        private void getTariff()
        {
            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand($"SELECT * FROM [Услуги]", sqlConnection);

            try
            {
                sqlReader = command.ExecuteReader();

                while (sqlReader.Read())
                {
                    tariffs.Add(Convert.ToDouble(sqlReader["Тариф"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "ЭТо бяда!!", ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (sqlReader != null)
                sqlReader.Close();
        }
        private void loadInfo()
        {
            SqlDataReader sqlReader = null;
            SqlCommand command;

            string personId = File.ReadAllText("Клиент.txt");

            command = new SqlCommand($"SELECT * FROM Клиенты", sqlConnection);

            sqlReader = command.ExecuteReader();

            while (sqlReader.Read())
            {
                if (Convert.ToString(sqlReader["ID"]) == personId)
                {
                    client.IdNumber = sqlReader.GetString(0);
                    client.FirstName = sqlReader.GetString(1);
                    client.SecondName = sqlReader.GetString(2);
                    client.ThirdName = sqlReader.GetString(3);
                    client.Address = sqlReader.GetString(4);
                    client.PhoneNumber = sqlReader.GetString(5);
                    client.Date = sqlReader.GetString(6);
                    client.PersonalAccount = sqlReader.GetString(7);
                    client.Photo = (byte[])sqlReader.GetValue(8);
                    break;
                }
            }

            if (sqlReader != null)
                sqlReader.Close();

            using (FileStream fs = new FileStream(@"..\..\img\imgClients\ClientPhoto.jpg", FileMode.OpenOrCreate))
            {
                fs.Write(client.Photo, 0, client.Photo.Length);
            }


            if (sqlReader != null)
                sqlReader.Close();
        }
    }
}
