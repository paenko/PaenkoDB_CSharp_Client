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
            ButtonPost.Click += (o, e) => PostContent();
            ButtonGet.Click += (o, e) => GetContent();
            ButtonRefresh.Click += (o, e) => Refresh();
            ButtonOpen.Click += (o, e) => OpenFile();
            ButtonPostFile.Click += (o, e) => PostPutFile(PaenkoDB.PaenkoDB.Method.Post);
            ButtonDelete.Click += (o, e) => DeleteDocument();
            ButtonPut.Click += (o, e) => PostPutFile(PaenkoDB.PaenkoDB.Method.Put);
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

        void PostContent()
        {
            Database.PostDocument(NodeBySelection(), PaenkoDocument.FromContent(MainContent.Text, Init.User, Init.Password), PaenkoDB.PaenkoDB.Method.Post);
        }

        void SelectLocation(object o, EventArgs e)
        {
            GetKeys();
        }

        void RefreshPeers()
        {
            Database.ClearNodes();
            LocationList.Items.Clear();
            foreach (PaenkoNode pn in Init.Peers)
            {
                LocationList.Items.Add(pn.Location);
                Database.AddNode(pn);
            }
        }

        void GetKeys()
        {
            KeyList.Items.Clear();
            List<string> keys = Database.GetKeys(NodeBySelection());
            foreach (string s in keys)
            {
                KeyList.Items.Add(s);
            }
        }

        PaenkoNode NodeBySelection()
        {
            return Init.Peers.Find(pn => pn.Location == (string)LocationList.SelectedItem);
        }
        string KeyBySelection()
        {
            return (string)KeyList.SelectedItem;
        }
    }
}
