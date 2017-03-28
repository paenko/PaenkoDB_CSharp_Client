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
using System.Resources;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        TextRange MainContent;
        Node OpenNode;
        UuidObject curLog;

        public Main(Node target, string LogID)
        {
            InitializeComponent();
            OpenNode = target;
            Authentication a = new Authentication(Init.User, Init.Password);
            OpenNode.Login(a);
            curLog = new UuidObject() { Id = LogID };
            MainContent = new TextRange(Output.Document.ContentStart, Output.Document.ContentEnd);
            MainContent.Text = $"" +
                $"Paenko Node {OpenNode.NodeLocation.ip} on port {OpenNode.NodeLocation.HttpPort} \r\n" +
                $"Country {OpenNode.NodeLocation.countryCode} : {OpenNode.NodeLocation.country}\r\n" +
                $"City {OpenNode.NodeLocation.city}\r\n" +
                $"Region {OpenNode.NodeLocation.region}\r\n" +
                $"Zipcode {OpenNode.NodeLocation.zip}\r\n" +
                $"Timezone {OpenNode.NodeLocation.timezone}\r\n" +
                $"Longitude {OpenNode.NodeLocation.lon}" +
                $"Latitude {OpenNode.NodeLocation.lat}";
            UpdateExplorer();
            FileExplorer.Drop += (o,e) => ExplorerDrop(e);
            FileExplorer.MouseMove += (o, e) => ExplorerDrag(e);
            FileExplorer.PreviewMouseLeftButtonDown += (o,e) => this.start = e.GetPosition(null);
            FileExplorer.MouseDoubleClick += (o, e) => ExplorerClick((Img)((ListBox)o).SelectedItem, e);
            BtnBeginT.Click += (o, e) => StartTransaction(Node.Command.Begin); 
            BtnCommT.Click += (o, e) => StartTransaction(Node.Command.Commit);
            BtnRollT.Click += (o, e) => StartTransaction(Node.Command.Rollback);
            BtnCommT.Visibility = Visibility.Hidden;
            BtnRollT.Visibility = Visibility.Hidden;
        }

        void StartTransaction(Node.Command command)
        {
            OpenNode.Transaction(curLog, command);
            if (command == Node.Command.Begin)
            {
                BtnBeginT.Visibility = Visibility.Hidden;
                BtnCommT.Visibility = Visibility.Visible;
                BtnRollT.Visibility = Visibility.Visible;
            }
            else
            {
                BtnBeginT.Visibility = Visibility.Visible;
                BtnCommT.Visibility = Visibility.Hidden;
                BtnRollT.Visibility = Visibility.Hidden;
            }
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
                    PostPutFile((bool)(tglMode.IsChecked) ? Node.Method.Put : Node.Method.Post, file);
                }
                UpdateExplorer();
            }
        }

        void UpdateExplorer()
        {
            FileExplorer.Items.Clear();
            foreach (string s in GetKeys())
            {
                string path = (File.Exists(s)) ? @"http://i.imgur.com/k1H1ACX.png" : @"http://i.imgur.com/KIsGDcz.png";
                Image I = new Image() { Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)), Width = 20, Height = 20 };
                FileExplorer.Items.Add(new Img(s, I));
            }
        }

        void PostPutFile(Node.Method m, string filename)
        {
            Document doc;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                doc = Document.FromStream(fs);
            }
            if (m == Node.Method.Put)
            {
                KeySelection ks = new KeySelection(GetKeys()); 
                ks.ShowDialog();
                if (ks.Selection == "") return;
                doc.id = ks.Selection;
            }
            OpenNode.PostDocument(doc, curLog, Guid.NewGuid().ToString());
        }

        void GetFile(string fileid)
        {
            var response = OpenNode.GetDocument(curLog, new UuidObject { Id = fileid });
            byte[] buffer;
            buffer = Convert.FromBase64String(response.payload);

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
            OpenNode.DeleteDocument(curLog , new UuidObject() { Id = fileid } );
            UpdateExplorer();
        }

        List<string> GetKeys()
        {
            return OpenNode.GetKeys(curLog);
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
