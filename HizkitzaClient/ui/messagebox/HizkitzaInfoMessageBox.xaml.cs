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
using static HizkitzaClient.ui.messagebox.HizkitzaBooleanMessageBox;

namespace HizkitzaClient.ui.messagebox
{
    public partial class HizkitzaInfoMessageBox : Window
    {
        public HizkitzaInfoMessageBox(string message)
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel(message, this);
        }

        public class MessageBoxViewModel(string message, Window window)
        {
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = message;
            public ICommand OkCommand { get; } = new RelayCommand(window.Close);
        }

        public static void ShowDialog(string msg)
        {
            new HizkitzaInfoMessageBox(msg).ShowDialog();
        }
    }
}
