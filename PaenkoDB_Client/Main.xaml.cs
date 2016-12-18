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
            FileExplorer.MouseMove += (o, e) => ExplorerDrag(e);
            FileExplorer.PreviewMouseLeftButtonDown += (o,e) => this.start = e.GetPosition(null);
            FileExplorer.MouseDoubleClick += (o, e) => ExplorerClick((Img)((ListBox)o).SelectedItem, e);
        }
        private Point start;

        void ExplorerDrag(MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = this.start - mpos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (this.FileExplorer.SelectedItems.Count == 0)
                {
                    return;
                }
                string[] files = new string[] { $"{AppDomain.CurrentDomain.BaseDirectory}{((Img)(FileExplorer.SelectedItem)).Str}" };
                string dataFormat = DataFormats.FileDrop;
                DataObject dataObject = new DataObject(dataFormat, files);
                DragDrop.DoDragDrop(this.FileExplorer, dataObject, DragDropEffects.Copy);
            }
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
                string path = (File.Exists(s)) ? @"C:\Users\Flori\Documents\DocumentIconYes.png" : @"C:\Users\Flori\Documents\DocumentIconNo.png";
                Image I = new Image() { Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)), Width = 20, Height=20 };
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
            byte[] buffer;
            buffer = Convert.FromBase64String(response.Document.payload);

            using (var fileStream = new FileStream($"{fileid}", FileMode.Create))
            using (var writeStream = new BinaryWriter(fileStream))
            {
                writeStream.Write(buffer);
            }
            //OpenFile(fileid);
        }

        void OpenFile(string fileid)
        {
            Process.Start($"{fileid}");
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
