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
            BtnSend.Click += (o, e) => PostPutFile((bool)(tglMode.IsChecked) ? PaenkoDB.PaenkoDB.Method.Put : PaenkoDB.PaenkoDB.Method.Post);
            BtnGet.Click += (o, e) => GetFile();
            BtnDelete.Click += (o, e) => DeleteDocument();
            MainContent.Text = $"" +
                $"Paenko Node {OpenNode.NodeLocation.ip} on port {OpenNode.NodeLocation.HttpPort} \r\n" +
                $"Country {OpenNode.NodeLocation.countryCode} : {OpenNode.NodeLocation.country}\r\n" +
                $"City {OpenNode.NodeLocation.city}\r\n" +
                $"Region {OpenNode.NodeLocation.region}\r\n" +
                $"Zipcode {OpenNode.NodeLocation.zip}\r\n" +
                $"Timezone {OpenNode.NodeLocation.timezone}\r\n" +
                $"";
        }

        void PostPutFile(PaenkoDB.PaenkoDB.Method m)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var doc = PaenkoDocument.Open(ofd.FileName, Init.User, Init.Password);
            if (m == PaenkoDB.PaenkoDB.Method.Put)
            {
                KeySelection ks = new KeySelection(GetKeys());
                ks.ShowDialog();
                if (ks.Selection == "") return;
                doc.id = ks.Selection;
            }
            Database.PostDocument(OpenNode, doc, m);
        }

        void GetFile()
        {
            KeySelection ks = new KeySelection(GetKeys());
            ks.ShowDialog();
            if (ks.Selection == "") return;
            var response = Database.GetDocument(OpenNode, ks.Selection);
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

        void DeleteDocument()
        {
            KeySelection ks = new KeySelection(GetKeys());
            ks.ShowDialog();
            if (ks.Selection == "") return;
            Database.DeleteDocument(OpenNode, ks.Selection);
        }

        List<string> GetKeys()
        {
            return Database.GetKeys(OpenNode);
        }
    }
}
