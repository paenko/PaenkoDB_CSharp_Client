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
using PaenkoDB;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        public enum Mode { Add, Edit };
        public Editor(Mode m = Mode.Add, PaenkoNode pn = null)
        {
            InitializeComponent();
            if (pn != null)
            {
                TextboxInputDes.Text = pn.Description;
                TextboxInputLoc.Text = pn.Location;
                TextboxInputId.Text = pn.ServerID.ToString();
            }

            ButtonConfirm.Click += (o, e) =>
            {
                if (m == Mode.Add)
                {
                    pn = new PaenkoNode(TextboxInputLoc.Text, ulong.Parse(TextboxInputId.Text), TextboxInputDes.Text);
                    Init.Peers.Add(pn);
                }
                else
                {
                    Init.Peers[Init.Peers.IndexOf(pn)] = new PaenkoNode(TextboxInputLoc.Text, ulong.Parse(TextboxInputId.Text), TextboxInputDes.Text);
                }
                Init.ListPeers();
                this.Close();
            };
        }
    }
}
