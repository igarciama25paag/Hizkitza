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
    public partial class StyledMessageBox : Window
    {
        public StyledMessageBox(string title, string message, string positive, string negative)
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel(title, message, positive, negative, this);
        }

        public class MessageBoxViewModel
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string PositiveText { get; set; }
            public string NegativeText { get; set; }
            public ICommand AcceptCommand { get; }
            public ICommand CancelCommand { get; }

            public MessageBoxViewModel(string title, string message, string positive, string negative, Window window)
            {
                Title = title;
                Message = message;
                PositiveText = positive;
                NegativeText = negative;

                AcceptCommand = new RelayCommand(() =>
                {
                    window.DialogResult = true;
                    window.Close();
                });

                CancelCommand = new RelayCommand(() =>
                {
                    window.DialogResult = false;
                    window.Close();
                });
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute) => _execute = execute;

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute();
            public event EventHandler CanExecuteChanged;
        }
    }
}
