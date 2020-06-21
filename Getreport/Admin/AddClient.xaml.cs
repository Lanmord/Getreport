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
using Microsoft.Win32;
using System.IO;

namespace Getreport.Admin
{
    /// <summary>
    /// Логика взаимодействия для AddClient.xaml
    /// </summary>
    public partial class AddClient : Page
    {
        List<Report> reports;
        List<Accrual> accruals;
        List<double> tariffs = new List<double>();

        SqlConnection sqlConnection;

        int monthNum = 1;
        int yearNum = 2020;
        string imagePath = "";

        public AddClient()
        {
            InitializeComponent();

            connectDB();

            reports = new List<Report>();

            List<int> itemYears = new List<int>();
            int year = 1995;
            DateTime now = DateTime.Now;

            for (int i = year; i <= now.Year; i++)
            {
                itemYears.Add(year);
                year++;
            }
            comboboxDate.ItemsSource = itemYears;
            comboboxDate.SelectedItem = now.Year;

            btn1.Content = "ЯНВ";
            btn2.Content = "ФЕВ";
            btn3.Content = "МАР";
            btn4.Content = "АПР";
            btn5.Content = "МАЙ";
            btn6.Content = "ИЮН";

            foreach (UIElement c in gridServ.Children)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).TextChanged += field1_TextChanged;
                }
            }

            foreach (UIElement c in radios.Children)
            {
                if (c is RadioButton)
                {
                    ((RadioButton)c).Click += toggleStatus_Click;
                }
            }

            foreach (UIElement c in mainStack.Children)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).TextChanged += firstBox_TextChanged;
                }
            }
            foreach (UIElement c in fioFields.Children)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).TextChanged += firstBox_TextChanged;
                }
            }

        }
        private async void connectDB()
        {

            string connectionStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kosty\source\repos\Getreport\Getreport\GetreportDB.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionStr);

            if (sqlConnection.State == ConnectionState.Closed)
                await sqlConnection.OpenAsync();

            getTariff();
        }
        private async void getTariff()
        {
            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand($"SELECT * FROM [Услуги]", sqlConnection);


            try
            {
                sqlReader = await command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
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

        private void addClientBD()
        {
            errorSameId.Visibility = Visibility.Collapsed;
            loading.Visibility = Visibility.Collapsed;

            SqlDataReader sqlReader = null;
            SqlCommand command;

            bool pass = true;
            string statusRep = "";
            int id1 = 0;
            int id2 = 0;

            command = new SqlCommand($"SELECT * FROM Клиенты", sqlConnection);
            sqlReader = command.ExecuteReader();

            while (sqlReader.Read())
            {
                if (Convert.ToString(sqlReader["ID"]) == personalIdBox.Text)
                {
                    pass = false;
                }
            }
            sqlReader.Close();

            if (pass)
            {
                loading.Visibility = Visibility.Visible;

                command = new SqlCommand($"INSERT Клиенты(ID,Имя,Фамилия,Отчество,Адрес,Телефон,[Дата рождения],[Лицевой счёт], Фото)" +
                        $"VALUES(N'{personalIdBox.Text}', N'{firstBox.Text}', N'{secondBox.Text}', N'{thirdBox.Text}', N'{addressBox.Text}', N'{phoneBox.Text}', '{dateBox.Text}', N'{personalIdBox.Text}', @Photo)", sqlConnection);

                byte[] imageData;
                if (imagePath == "") imagePath = @"..\..\img\Nopic.jpg";
                using (FileStream fs = new FileStream(imagePath, FileMode.Open))
                {
                    imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, imageData.Length);
                }

                command.Parameters.AddWithValue("Photo", imageData);
                command.ExecuteNonQuery();

                File.WriteAllText("Клиент.txt", personalIdBox.Text);

                if (reports.Count != 0)
                {

                    for (int i = 0; i < reports.Count; i++)
                    {
                        statusRep = reports[i].Status ? "оплачено" : "не оплачено";
                        command = new SqlCommand($"INSERT Отчёты([ID Клиента],Месяц,Год,Льгота,[Сумма задолженности],[Итоговая сумма],[Сумма льгот],Статус)" +
                        $"VALUES(N'{personalIdBox.Text}', '{reports[i].Month}', '{reports[i].Year}', '{reports[i].Privilege}', " +
                        $"@Долг, @Итог, @Льготы, N'{statusRep}')", sqlConnection);

                        command.Parameters.AddWithValue("Долг", reports[i].Debt);
                        command.Parameters.AddWithValue("Итог", reports[i].TotalAmount);
                        command.Parameters.AddWithValue("Льготы", reports[i].PrivilegeAmount);

                        command.ExecuteNonQuery();

                        command = new SqlCommand($"SELECT @@IDENTITY AS [ID]", sqlConnection);
                        sqlReader = command.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            id1 = Convert.ToInt32(sqlReader["ID"]);
                        }
                        sqlReader.Close();

                        for (int j = 0; j < reports[i].Accruals.Count; j++)
                        {
                            command = new SqlCommand($"INSERT Начисления([ID услуги],[Начислено],[Объём(кол-во)])" +
                        $"VALUES('{(j + 1)}', @Начислено, @Кол)", sqlConnection);

                            command.Parameters.AddWithValue("Начислено", reports[i].Accruals[j].Accrued);
                            command.Parameters.AddWithValue("Кол", reports[i].Accruals[j].Amount);
                            command.ExecuteNonQuery();

                            command = new SqlCommand($"SELECT @@IDENTITY AS [ID]", sqlConnection);
                            sqlReader = command.ExecuteReader();

                            while (sqlReader.Read())
                            {
                                id2 = Convert.ToInt32(sqlReader["ID"]);
                            }
                            sqlReader.Close();

                            command = new SqlCommand($"INSERT [Отчёты начислений]([ID начисления],[ID отчёта])" +
                            $"VALUES(@id2, @id1)", sqlConnection);

                            command.Parameters.AddWithValue("id2", id2);
                            command.Parameters.AddWithValue("id1", id1);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                loading.Visibility = Visibility.Collapsed;

                App.Current.MainWindow.Content = new ClientPageAdmin();
            }
            else
            {
                errorSameId.Visibility = Visibility.Visible;
            }

            if (sqlReader != null)
                sqlReader.Close();
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            errorPhone.Visibility = Visibility.Collapsed;

            errorFIO.Visibility = Visibility.Collapsed;

            errorDate.Visibility = Visibility.Collapsed;

            int countPass = 0;

            Regex regex = new Regex(@"\n[А-яё\-]+\n");

            string str = $"\n{firstBox.Text}\n\n{secondBox.Text}\n\n{thirdBox.Text}\n";

            if (regex.Matches(str).Count == 3)
            {
                countPass++;
            }
            else
            {
                errorFIO.Visibility = Visibility.Visible;
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

            DateTime date = DateTime.Now;
            regex = new Regex(@"^[0-9][0-9]\.[0-9][0-9]\.[0-9][0-9][0-9][0-9]$");

            if (regex.Matches(dateBox.Text).Count == 1)
            {
                try
                {
                    var dateBoxSplit = dateBox.Text.Split('.');
                    DateTime date2 = new DateTime(int.Parse(dateBoxSplit[2]), int.Parse(dateBoxSplit[1]), int.Parse(dateBoxSplit[0]));
                    if (date2 < date)
                    {
                        countPass++;
                    }
                    else
                    {
                        errorDate.Visibility = Visibility.Visible;
                    }

                }
                catch (Exception)
                {
                    errorDate.Visibility = Visibility.Visible;
                }
            }
            else
            {
                errorDate.Visibility = Visibility.Visible;
            }

            if (countPass == 3)
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                    addClientBD();
            }
        }
        private void btnComplite_Click(object sender, RoutedEventArgs e)
        {
            errorIncorrectSymbols.Visibility = Visibility.Collapsed;

            Regex regex = new Regex(@"[^0-9\,]+");

            if (regex.Matches(field1.Text).Count == 0 && regex.Matches(field2.Text).Count == 0 && regex.Matches(field3.Text).Count == 0 &&
                 regex.Matches(field4.Text).Count == 0 && regex.Matches(field5.Text).Count == 0 && regex.Matches(field6.Text).Count == 0 &&
                 regex.Matches(field7.Text).Count == 0 && regex.Matches(field8.Text).Count == 0 && regex.Matches(field9.Text).Count == 0 &&
                 regex.Matches(field10.Text).Count == 0)
            {
                if (tariffs.Count > 0)
                {
                    bool pass = true;
                    int index = 0;

                    for (int i = 0; i < reports.Count; i++)
                    {
                        if (reports[i].Year == yearNum && reports[i].Month == monthNum)
                        {
                            pass = false;
                            index = i;
                        }
                    }

                    if (pass)
                    {
                        accruals = new List<Accrual>();

                        accruals.Add(new Accrual(serv1.Text, Convert.ToDouble(field1.Text), tariffs[0]));
                        accruals.Add(new Accrual(serv2.Text, Convert.ToDouble(field2.Text), tariffs[1]));
                        accruals.Add(new Accrual(serv3.Text, Convert.ToDouble(field3.Text), tariffs[2]));
                        accruals.Add(new Accrual(serv4.Text, Convert.ToDouble(field4.Text), tariffs[3]));
                        accruals.Add(new Accrual(serv5.Text, Convert.ToDouble(field5.Text), tariffs[4]));
                        accruals.Add(new Accrual(serv6.Text, Convert.ToDouble(field6.Text), tariffs[5]));
                        accruals.Add(new Accrual(serv7.Text, Convert.ToDouble(field7.Text), tariffs[6]));
                        accruals.Add(new Accrual(serv8.Text, Convert.ToDouble(field8.Text), tariffs[7]));
                        accruals.Add(new Accrual("Электроэнергия на работу лифтов", Convert.ToDouble(field9.Text), tariffs[8]));
                        accruals.Add(new Accrual(serv10.Text, Convert.ToDouble(field10.Text), tariffs[9]));

                        int privilege = 0;

                        if (radio1.IsChecked == true) privilege = 0;
                        if (radio2.IsChecked == true) privilege = 20;
                        if (radio3.IsChecked == true) privilege = 40;
                        if (radio4.IsChecked == true) privilege = 60;
                        if (radio5.IsChecked == true) privilege = 80;
                        if (radio6.IsChecked == true) privilege = 100;

                        reports.Add(new Report(yearNum, monthNum, privilege, Convert.ToBoolean(toggleStatus.IsChecked), accruals, reports));

                        btnComplite.IsEnabled = false;
                    }
                    else
                    {
                        accruals[0] = (new Accrual(serv1.Text, Convert.ToDouble(field1.Text), tariffs[0]));
                        accruals[1] = (new Accrual(serv2.Text, Convert.ToDouble(field2.Text), tariffs[1]));
                        accruals[2] = (new Accrual(serv3.Text, Convert.ToDouble(field3.Text), tariffs[2]));
                        accruals[3] = (new Accrual(serv4.Text, Convert.ToDouble(field4.Text), tariffs[3]));
                        accruals[4] = (new Accrual(serv5.Text, Convert.ToDouble(field5.Text), tariffs[4]));
                        accruals[5] = (new Accrual(serv6.Text, Convert.ToDouble(field6.Text), tariffs[5]));
                        accruals[6] = (new Accrual(serv7.Text, Convert.ToDouble(field7.Text), tariffs[6]));
                        accruals[7] = (new Accrual(serv8.Text, Convert.ToDouble(field8.Text), tariffs[7]));
                        accruals[8] = (new Accrual("Электроэнергия на работу лифтов", Convert.ToDouble(field9.Text), tariffs[8]));
                        accruals[9] = (new Accrual(serv10.Text, Convert.ToDouble(field10.Text), tariffs[9]));

                        int privilege = 0;

                        if (radio1.IsChecked == true) privilege = 0;
                        if (radio2.IsChecked == true) privilege = 20;
                        if (radio3.IsChecked == true) privilege = 40;
                        if (radio4.IsChecked == true) privilege = 60;
                        if (radio5.IsChecked == true) privilege = 80;
                        if (radio6.IsChecked == true) privilege = 100;

                        reports[index] = (new Report(yearNum, monthNum, privilege, Convert.ToBoolean(toggleStatus.IsChecked), accruals, reports));

                        btnComplite.IsEnabled = false;
                    }
                    //for (int i = 0; i < reports.Count; i++)
                    //{
                    //    MessageBox.Show("Всего начислено = " + Convert.ToString(reports[i].AmountAccrual) + " " + "Одно из начислений = " + Convert.ToString(reports[i].Accruals[3].Accrued) + " " + "Итоговая сумма = " + Convert.ToString(reports[i].TotalAmount) + " " + "Сумма привелегий = " + Convert.ToString(reports[i].PrivilegeAmount) + " " + "Льгота% = " + Convert.ToString(reports[i].Privilege) + " " + "Долг = " + Convert.ToString(reports[i].Debt));

                    //}
                    //MessageBox.Show(Convert.ToString(reports[0].AmountAccrual) + " " + Convert.ToString(reports[0].Accruals[0].Accrued) + " | " + Convert.ToString(reports[0].TotalAmount) + " " + Convert.ToString(reports[0].PrivilegeAmount));
                    //MessageBox.Show(Convert.ToString(reports[0].Status));
                }
            }
            else
            {
                errorIncorrectSymbols.Visibility = Visibility.Visible;
            }

        }
        private void btn_front_Click(object sender, RoutedEventArgs e)
        {
            btn_back.IsEnabled = true;

            clearBtn();

            btn1.Content = "ИЮЛ";
            btn2.Content = "АВГ";
            btn3.Content = "СЕН";
            btn4.Content = "ОКТ";
            btn5.Content = "НОЯ";
            btn6.Content = "ДЕК";

            if (isRightMonth(Convert.ToString(btn1.Content))) btn1.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn2.Content))) btn2.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn3.Content))) btn3.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn4.Content))) btn4.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn5.Content))) btn5.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn6.Content))) btn6.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));

            btn_front.IsEnabled = false;
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            btn_front.IsEnabled = true;

            clearBtn();

            btn1.Content = "ЯНВ";
            btn2.Content = "ФЕВ";
            btn3.Content = "МАР";
            btn4.Content = "АПР";
            btn5.Content = "МАЙ";
            btn6.Content = "ИЮН";

            if (isRightMonth(Convert.ToString(btn1.Content))) btn1.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn2.Content))) btn2.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn3.Content))) btn3.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn4.Content))) btn4.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn5.Content))) btn5.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            if (isRightMonth(Convert.ToString(btn6.Content))) btn6.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));

            btn_back.IsEnabled = false;
        }

        private bool isRightMonth(string m)
        {
            switch (m)
            {
                case "ЯНВ": return monthNum == 1;
                case "ФЕВ": return monthNum == 2;
                case "МАР": return monthNum == 3;
                case "АПР": return monthNum == 4;
                case "МАЙ": return monthNum == 5;
                case "ИЮН": return monthNum == 6;
                case "ИЮЛ": return monthNum == 7;
                case "АВГ": return monthNum == 8;
                case "СЕН": return monthNum == 9;
                case "ОКТ": return monthNum == 10;
                case "НОЯ": return monthNum == 11;
                case "ДЕК": return monthNum == 12;
            }

            return false;
        }

        private void monthInit(string m)
        {
            switch (m)
            {
                case "ЯНВ": monthNum = 1; break;
                case "ФЕВ": monthNum = 2; break;
                case "МАР": monthNum = 3; break;
                case "АПР": monthNum = 4; break;
                case "МАЙ": monthNum = 5; break;
                case "ИЮН": monthNum = 6; break;
                case "ИЮЛ": monthNum = 7; break;
                case "АВГ": monthNum = 8; break;
                case "СЕН": monthNum = 9; break;
                case "ОКТ": monthNum = 10; break;
                case "НОЯ": monthNum = 11; break;
                case "ДЕК": monthNum = 12; break;
            }

        }
        private void clearAll()
        {
            toggleStatus.IsChecked = false;
            radio1.IsChecked = true;

            foreach (UIElement c in gridServ.Children)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).Clear();
                }
            }

        }
        private void clearBtn()
        {
            btn1.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn2.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn3.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn4.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn5.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn6.Background = new SolidColorBrush(Color.FromRgb(109, 99, 153));
            btn1.IsEnabled = true;
            btn2.IsEnabled = true;
            btn3.IsEnabled = true;
            btn4.IsEnabled = true;
            btn5.IsEnabled = true;
            btn6.IsEnabled = true;
        }
        private void fillAll()
        {
            int index = 0;
            bool isFind = false;
            int j = 0;

            for (int i = 0; i < reports.Count; i++)
            {
                if (reports[i].Year == yearNum && reports[i].Month == monthNum)
                {
                    index = i;
                    isFind = true;
                    break;
                }
            }
            if (isFind)
            {
                toggleStatus.IsChecked = reports[index].Status;
                if (reports[index].Privilege == 0) radio1.IsChecked = true;
                if (reports[index].Privilege == 20) radio2.IsChecked = true;
                if (reports[index].Privilege == 40) radio3.IsChecked = true;
                if (reports[index].Privilege == 60) radio4.IsChecked = true;
                if (reports[index].Privilege == 80) radio5.IsChecked = true;
                if (reports[index].Privilege == 100) radio6.IsChecked = true;

                foreach (UIElement c in gridServ.Children)
                {
                    if (c is TextBox)
                    {
                        ((TextBox)c).Text = Convert.ToString(reports[index].Accruals[j].Amount);
                        j++;
                    }

                }

                btnComplite.IsEnabled = false;
            }
        }
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn1.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn1.Content));
            clearAll();
            fillAll();

            btn1.IsEnabled = false;
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn2.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn2.Content));
            clearAll();
            fillAll();
            btn2.IsEnabled = false;
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn3.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn3.Content));
            clearAll();
            fillAll();
            btn3.IsEnabled = false;
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn4.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn4.Content));
            clearAll();
            fillAll();
            btn4.IsEnabled = false;
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn5.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn5.Content));
            clearAll();
            fillAll();
            btn5.IsEnabled = false;
        }

        private void btn6_Click(object sender, RoutedEventArgs e)
        {
            clearBtn();

            btn6.Background = new SolidColorBrush(Color.FromRgb(130, 63, 114));
            monthInit(Convert.ToString(btn6.Content));
            clearAll();
            fillAll();
            btn6.IsEnabled = false;
        }

        private void comboboxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            yearNum = Convert.ToInt32(comboboxDate.SelectedValue);
            clearAll();
            fillAll();
        }

        private void isEmptyFields()
        {
            if (field1.Text != "" && field2.Text != "" && field3.Text != "" && field4.Text != "" && field5.Text != "" &&
                field6.Text != "" && field7.Text != "" && field8.Text != "" && field9.Text != "" && field10.Text != "")
            {
                btnComplite.IsEnabled = true;
            }
            else
            {
                btnComplite.IsEnabled = false;
            }
        }

        private bool isEmptyFields2()
        {
            if (field1.Text != "" && field2.Text != "" && field3.Text != "" && field4.Text != "" && field5.Text != "" &&
                field6.Text != "" && field7.Text != "" && field8.Text != "" && field9.Text != "" && field10.Text != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void field1_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEmptyFields();
        }

        private void toggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (isEmptyFields2())
            {
                btnComplite.IsEnabled = true;
            }
        }

        private void checkEmptyFields()
        {
            if (firstBox.Text != "" && secondBox.Text != "" && thirdBox.Text != "" && phoneBox.Text != "" && dateBox.Text != "")
            {
                btnAdd.IsEnabled = true;
            }
            else
            {
                btnAdd.IsEnabled = false;
            }
        }

        private void firstBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkEmptyFields();
        }

        private void btnDeleteReport_Click(object sender, RoutedEventArgs e)
        {
            errorIncorrectSymbols.Visibility = Visibility.Collapsed;

            for (int i = 0; i < reports.Count; i++)
            {
                if (reports[i].Year == yearNum && reports[i].Month == monthNum)
                {
                    reports.RemoveAt(i);
                    break;
                }
            }

            clearAll();
        }

        private void loadPhoto_Click(object sender, RoutedEventArgs e)
        {
            
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "(.jpg)|*.jpg|(.png)|*.png|(.tiff)|*.tiff"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                imageIcon.Visibility = Visibility.Hidden;

                image.Source = BitmapFromUri(new Uri(dlg.FileName));

                imagePath = dlg.FileName;
            }
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
    }
}
