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

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for KeySelection.xaml
    /// </summary>
    public partial class KeySelection : Window
    {
        public string Selection { get; set; } = "";
        public KeySelection(List<string> keys)
        {
            InitializeComponent();
            foreach (string s in keys)
                KeySelect.Items.Add(new ListItem(s, () => { Selection = s; this.Close(); }));
        }
    }
}
