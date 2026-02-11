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
    public partial class HizkitzaArgMessageBox : Window
    {
        public HizkitzaArgMessageBox(string message, string positive, string negative)
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel(message, positive, negative, "", this);
        }

        public class MessageBoxViewModel
        {
            private readonly Window _window;

            public string Title { get; set; } = string.Empty;
            public string Message { get; set; }
            public string PositiveText { get; set; }
            public string NegativeText { get; set; }
            public string InputValue { get; set; }

            public ICommand AcceptCommand { get; }
            public ICommand CancelCommand { get; }

            public MessageBoxViewModel(string message, string positive, string negative, string defaultValue, Window window)
            {
                Message = message;
                PositiveText = positive;
                NegativeText = negative;
                InputValue = defaultValue;
                _window = window;

                AcceptCommand = new RelayCommand(() =>
                {
                    _window.DialogResult = true;
                    _window.Close();
                });

                CancelCommand = new RelayCommand(() =>
                {
                    _window.DialogResult = false;
                    _window.Close();
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

        public static string? ShowDialog(string msg)
        {
            var msgbox = new HizkitzaArgMessageBox(msg, "Ados", "Utzi");
            if (msgbox.ShowDialog() == true)
            {
                var viewModel = (MessageBoxViewModel)msgbox.DataContext;
                return viewModel.InputValue;
            }
            return null;
        }
    }
}
