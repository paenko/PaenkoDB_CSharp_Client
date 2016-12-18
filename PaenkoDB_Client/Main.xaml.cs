using Microsoft.Win32;
using PaenkoDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public static PaenkoDB.PaenkoDB Database = new PaenkoDB.PaenkoDB();
        TextRange MainContent;
        PaenkoNode OpenNode;
        public Main(PaenkoNode target)
        {
            InitializeComponent();
            OpenNode = target;
            MainContent = new TextRange(Output.Document.ContentStart, Output.Document.ContentEnd);
            MainContent.Text = $"" +
                $"Paenko Node {OpenNode.NodeLocation.ip} on port {OpenNode.NodeLocation.HttpPort} \r\n" +
                $"Country {OpenNode.NodeLocation.countryCode} : {OpenNode.NodeLocation.country}\r\n" +
                $"City {OpenNode.NodeLocation.city}\r\n" +
                $"Region {OpenNode.NodeLocation.region}\r\n" +
                $"Zipcode {OpenNode.NodeLocation.zip}\r\n" +
                $"Timezone {OpenNode.NodeLocation.timezone}\r\n" +
                $"";
            UpdateExplorer();
            FileExplorer.Drop += (o,e) => ExplorerDrop(e);
            FileExplorer.MouseDoubleClick += (o, e) => ExplorerClick((Img)((ListBox)o).SelectedItem, e);
        }

        void ExplorerClick(Img o, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                DeleteDocument(o.Str);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                GetFile(o.Str);
            }
            UpdateExplorer();
        }

        void ExplorerDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    PostPutFile((bool)(tglMode.IsChecked) ? PaenkoDB.PaenkoDB.Method.Put : PaenkoDB.PaenkoDB.Method.Post, file);
                }
                UpdateExplorer();
            }
        }

        void UpdateExplorer()
        {
            FileExplorer.Items.Clear();
            foreach (string s in GetKeys())
            {
                Image I = new Image() { Source = new BitmapImage(new Uri(@"C:\Users\Flori\Documents\DocumentIcon.png", UriKind.RelativeOrAbsolute)), Width = 20, Height=20 };
                FileExplorer.Items.Add(new Img(s, I));
            }
        }

        void PostPutFile(PaenkoDB.PaenkoDB.Method m, string filename)
        {
            var doc = PaenkoDocument.Open(filename, Init.User, Init.Password);
            if (m == PaenkoDB.PaenkoDB.Method.Put)
            {
                KeySelection ks = new KeySelection(GetKeys()); 
                ks.ShowDialog();
                if (ks.Selection == "") return;
                doc.id = ks.Selection;
            }
            Database.PostDocument(OpenNode, doc, m);
        }
        void GetFile(string fileid)
        {
            var response = Database.GetDocument(OpenNode, fileid);
            int fileCount = 0;
            byte[] buffer;
            while (File.Exists($"Document{fileCount}")) { fileCount++; }

            buffer = Convert.FromBase64String(response.Document.payload);

            using (var fileStream = new FileStream($"Document{fileCount}", FileMode.Create))
            using (var writeStream = new BinaryWriter(fileStream))
            {
                writeStream.Write(buffer);
            }
            OpenFile();
        }

        void OpenFile()
        {
            int fileCount = 0;
            while (File.Exists($"Document{fileCount}")) { fileCount++; }
            Process.Start($"Document{fileCount - 1}");
        }

        void DeleteDocument(string fileid)
        {
            Database.DeleteDocument(OpenNode, fileid);
        }

        List<string> GetKeys()
        {
            return Database.GetKeys(OpenNode);
        }
    }

    public class Img
    {
        public Img(string value, Image img) { Str = value; Image = img; }
        public string Str { get; set; }
        public Image Image { get; set; }
        public string ShowStr { get { return Str.Substring(0, 5); } }
    }
}
