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

namespace HizkitzaClient.ui.window.page
{
    /// <summary>
    /// Interaction logic for PlayerLobby.xaml
    /// </summary>
    public partial class PlayerLobby : Page
    {
        public PlayerLobby()
        {
            InitializeComponent();
            var item1 = new ListBoxItem();
            item1.Content = "Iker Partida";
            var item2 = new ListBoxItem();
            item2.Content = "Los Pulentos";
            var item3 = new ListBoxItem();
            item3.Content = "xX_SuperHitzak_Xx";

            partidak.Items.Add(item1);
            partidak.Items.Add(item2);
            partidak.Items.Add(item3);

            itxura_combobox.Items.Add("Itxura 1");
            itxura_combobox.Items.Add("Itxura 2");
            itxura_combobox.Items.Add("Itxura 3");
        }
    }
}
