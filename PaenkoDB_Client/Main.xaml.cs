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
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Threading;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public static PaenkoDB.PaenkoDB Database = new PaenkoDB.PaenkoDB();
        TextRange MainContent, SecondaryContent;
        DispatcherTimer Ticker = new DispatcherTimer();
        public Main()
        {
            InitializeComponent();
            MainContent = new TextRange(TextContent.Document.ContentStart, TextContent.Document.ContentEnd);
            SecondaryContent = new TextRange(TextContentSecondary.Document.ContentStart, TextContentSecondary.Document.ContentEnd);
            RefreshPeers();
            Ticker.Interval = new TimeSpan(0, 0, 5);
            Ticker.Tick += (o, e) => Refresh();
            RTBContent.Click += (o, e) => TabContent();
            RTBDocument.Click += (o, e) => TabDocument();
            LocationList.SelectionChanged += (o, e) => SelectLocation(o, e);
            ButtonPost.Click += (o, e) => PostPutContent(PaenkoDB.PaenkoDB.Method.Post);
            ButtonGet.Click += (o, e) => GetContent();
            ButtonRefresh.Click += (o, e) => Refresh();
            ButtonOpen.Click += (o, e) => OpenFile();
            ButtonPostFile.Click += (o, e) => PostPutFile(PaenkoDB.PaenkoDB.Method.Post);
            ButtonDelete.Click += (o, e) => DeleteDocument();
            ButtonPutFile.Click += (o, e) => PostPutFile(PaenkoDB.PaenkoDB.Method.Put);
            ButtonPut.Click += (o, e) => PostPutContent(PaenkoDB.PaenkoDB.Method.Put);
        }

        void TabContent()
        {
            TextContent.Visibility = Visibility.Visible;
            TextContentSecondary.Visibility = Visibility.Hidden;
        }

        void TabDocument()
        {
            TextContent.Visibility = Visibility.Hidden;
            TextContentSecondary.Visibility = Visibility.Visible;
        }

        void DeleteDocument()
        {
            Database.DeleteDocument(NodeBySelection(), KeyBySelection());
        }

        void PostPutFile(PaenkoDB.PaenkoDB.Method m)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var doc = PaenkoDocument.Open(ofd.FileName, Init.User, Init.Password);
            if (m == PaenkoDB.PaenkoDB.Method.Put) doc.id = KeyBySelection();
            Database.PostDocument(NodeBySelection(), doc, m);
        }

        void OpenFile()
        {
            int fileCount = 0;
            while (File.Exists($"Document{fileCount}")) { fileCount++; }
            Process.Start($"Document{fileCount-1}");
        }

        void Refresh()
        {
            GetKeys();
        }

        void GetContent()
        {
            var response = Database.GetDocument(NodeBySelection(), KeyBySelection());
            int fileCount = 0;
            byte[] buffer;
            while (File.Exists($"Document{fileCount}")) { fileCount++; }
            
            buffer = Convert.FromBase64String(response.Document.payload);
            MainContent.Text = Encoding.Default.GetString(buffer);
            
            buffer = Convert.FromBase64String(response.Document.payload);
            SecondaryContent.Text = JsonHelper.FormatJson(response.RAW);
            
            using (var fileStream = new FileStream($"Document{fileCount}", FileMode.Create))
            using (var writeStream = new BinaryWriter(fileStream))
            {
                writeStream.Write(buffer);
            }
        }
        
        void PostPutContent(PaenkoDB.PaenkoDB.Method m)
        {
            var doc = PaenkoDocument.FromContent(MainContent.Text, Init.User, Init.Password);
            if (m == PaenkoDB.PaenkoDB.Method.Put) doc.id = KeyBySelection();
            Database.PostDocument(NodeBySelection(), doc, m);
        }

        void SelectLocation(object o, EventArgs e)
        {
            GetKeys();
        }

        void RefreshPeers()
        {
            LocationList.Items.Clear();
            foreach (PaenkoNode pn in Init.Peers)
            {
                LocationList.Items.Add(new ListItem(pn.NodeLocation.ip));
            }
        }

        void GetKeys()
        {
            KeyList.Items.Clear();
            List<string> keys = Database.GetKeys(NodeBySelection());
            foreach (string s in keys)
            {
                KeyList.Items.Add(new ListItem(s));
            }
        }

        PaenkoNode NodeBySelection()
        {
            return Init.Peers.Find(pn => pn.NodeLocation.ip == (string)((ListItem)LocationList.SelectedItem).Content);
        }
        string KeyBySelection()
        {
            return (string)((ListItem)KeyList.SelectedItem).Content;
        }
    }
    class ListItem:ListBoxItem
    {
        public ListItem(string content):base()
        {
            Content = content;
            FontSize = 14;
        }
    }
}
