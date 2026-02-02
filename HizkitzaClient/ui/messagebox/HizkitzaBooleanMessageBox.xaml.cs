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

namespace HizkitzaClient.ui.messagebox
{
    /// <summary>
    /// Interaction logic for StyledMessageBox.xaml
    /// </summary>
    public partial class HizkitzaBooleanMessageBox : Window
    {
        public HizkitzaBooleanMessageBox(string message, string positive, string negative)
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel(message, positive, negative, this);
        }

        public class MessageBoxViewModel(string message, string positive, string negative, Window window)
        {
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = message;
            public string PositiveText { get; set; } = positive;
            public string NegativeText { get; set; } = negative;
            public ICommand AcceptCommand { get; } = new RelayCommand(() =>
                {
                    window.DialogResult = true;
                    window.Close();
                });
            public ICommand CancelCommand { get; } = new RelayCommand(() =>
                {
                    window.DialogResult = false;
                    window.Close();
                });
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute) => _execute = execute;

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute();
            public event EventHandler CanExecuteChanged;
        }

        public static HizkitzaBooleanMessageBox ShowDialog(string msg)
        {
            var msgbox = new HizkitzaBooleanMessageBox(msg, "Bai", "Ez");
            msgbox.ShowDialog();
            return msgbox;
        }
    }
}
