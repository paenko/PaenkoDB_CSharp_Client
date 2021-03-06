﻿using System;
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
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Init.xaml
    /// </summary>
    public partial class Init : Window
    {
        public static List<Node> Peers = new List<Node>();
        public static string User = "default", Password = "default";
        public static ListBox PeerOutput;
        public Init()
        {
            InitializeComponent();
            PeerOutput = DataPeerlist;
            // Events

            ButtonAdd.Click += (o, e) => AddPeer();
            ButtonEdit.Click += (o, e) => EditPeer();
            ButtonRemove.Click += (o, e) => RemovePeer();
            ButtonConnect.Click += (o, e) => Connect();

            // Load Peers from file
            /*
            if (File.Exists("Peerlist.json"))
            {
                using (var stream = new FileStream("Peerlist.json", FileMode.Open))
                using (var readStream = new StreamReader(stream))
                {
                    string file = readStream.ReadToEnd();
                    Peers = JsonConvert.DeserializeObject<List<Node>>(file);
                }
            }*/

            ListPeers();
        }

        public static void ListPeers()
        {
            PeerOutput.Items.Clear();
            foreach (Node pn in Peers)
            {
                PeerOutput.Items.Add(new ListItem(pn.NodeLocation.ip, null));
            }
        }

        void AddPeer()
        {
            Editor addEditor = new Editor();
            addEditor.Show();
        }

        void EditPeer()
        {
            Editor editEditor = new Editor(Editor.Mode.Edit, Peers[PeerOutput.SelectedIndex]);
            editEditor.Show();
        }

        void RemovePeer()
        {
            Peers.RemoveAt(PeerOutput.SelectedIndex);
            ListPeers();
        }

        void Connect()
        {
            // Save Peers to file
            /*
            string json = JsonConvert.SerializeObject(Peers);
            using (var stream = new FileStream("Peerlist.json", FileMode.Create))
            using (var writeStream = new StreamWriter(stream))
            {
                writeStream.Write(json);
            }*/

            //Main MainWindow = new Main();
            //MainWindow.Show();
            User = TextUser.Text;
            Password = TextPassword.Password;
            Interactive InteractiveWin = new Interactive();
            InteractiveWin.Show();
        }
    }
    class ListItem : ListBoxItem
    {
        public ListItem(string content, Action Dmc) : base()
        {
            Content = content;
            FontSize = 14;
            this.MouseDoubleClick += (o, e) => Dmc();
        }
    }
}
