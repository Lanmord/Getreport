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

namespace Getreport.Admin
{
    /// <summary>
    /// Логика взаимодействия для MainMenuAdmin.xaml
    /// </summary>
    public partial class MainMenuAdmin : Page
    {
        public MainMenuAdmin()
        {
            InitializeComponent();
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            loupe.Visibility = Visibility.Hidden;
            //if (search.Text == "") loupe.Visibility = Visibility.Visible;
            //if(search.Text == "К")
            //{
            //    for (int i = 0; i < listClients.Items.Count; i++)
            //    {
            //        if(listClients.Items[i]
            //    }
            //}    
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Content = new Autorisation();
        }
    }
}
