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

namespace Server_performant_Taskbased
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Button_Click(null, null);
        }

        Listener listener;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listener = new Listener(34700);
            button_StartServer.Content = "Server started ...";

            Console.WriteLine("___________________");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listener.clients.Count; i++)
            {
                listener.clients[i].SendMessage("common mesage from server... "+DateTime.Now.ToString());
            }
        }
    }
}
