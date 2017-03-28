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
using System.Net;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        public enum Mode { Add, Edit };
        public Editor(Mode m = Mode.Add, Node pn = null)
        {
            InitializeComponent();
            if (pn != null)
            {
                TextboxInputIp.Text = pn.NodeLocation.ip;
                TextboxInputPort.Text = Convert.ToString(pn.NodeLocation.HttpPort);
            }

            ButtonConfirm.Click += (o, e) =>
            {
                if (m == Mode.Add)
                {
                    pn = new Node(IPAddress.Parse(TextboxInputIp.Text), int.Parse(TextboxInputPort.Text), (bool)CheckLocation.IsChecked);
                    Init.Peers.Add(pn);
                }
                else
                {
                    Init.Peers[Init.Peers.IndexOf(pn)] = new Node(IPAddress.Parse(TextboxInputIp.Text), int.Parse(TextboxInputPort.Text));
                }
                Init.ListPeers();
                this.Close();
            };
        }
    }
}
