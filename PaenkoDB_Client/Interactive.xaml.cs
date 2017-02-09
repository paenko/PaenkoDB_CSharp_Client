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
using GoogleMapsApi;
using PaenkoDB;

namespace PaenkoDB_Client
{
    /// <summary>
    /// Interaction logic for Interactive.xaml
    /// </summary>
    public partial class Interactive : Window, IGoogleMapHost
    {
        IGoogleMapWrapper Map;
        PaenkoDB.PaenkoDB Database;
        public Interactive()
        {
            InitializeComponent();
            Map = GoogleMapWrapper.Create(this);
            Database = new PaenkoDB.PaenkoDB();
            Init.Peers.ForEach(pn => { if (pn.NodeLocation.lon == 0 && pn.NodeLocation.lat == 0) pn.NodeLocation.lat = 48; pn.NodeLocation.lon = 16; });
            Map.ApiReady += () => DrawNodes();
        }

        async void DrawNodes()
        {
            List<PaenkoNode> dead = (await Database.CheckNodeStatusAsync(Init.Peers));
            List<PaenkoNode> alive = Init.Peers.Except(dead).AsEnumerable().ToList();
            MarkerOptions moAlive = new MarkerOptions() { Icon = @"http://i.imgur.com/JPV3KXR.png", Clickable = true, DraggingEnabled = false, Flat = false, Optimized = true, RaiseOnDrag = true };
            MarkerOptions moDead = new MarkerOptions() { Icon = @"http://i.imgur.com/WpTAa9N.png", Clickable = true, DraggingEnabled = false, Flat = false, Optimized = true, RaiseOnDrag = true };
            foreach (PaenkoNode pn in Init.Peers)
            {
                var Marker = Map.AddMarker(new GeographicLocation(pn.NodeLocation.lat, pn.NodeLocation.lon), (alive.Contains(pn) ? moAlive : moDead));
                Marker.Click += (im ,gl) =>
                {
                    KeySelection ks = new KeySelection(Database.GetLogs(pn));
                    ks.ShowDialog();
                    Main m = new Main(pn, ks.Selection);
                    m.Show();
                };
            }
        }

        public void RegisterScriptingObject(IGoogleMapRequired wrapper)
        {
            Browser.ObjectForScripting = wrapper;
        }

        public void SetHostDocumentText(string text)
        {
            Browser.NavigateToString(text);
        }

        public object InvokeScript(string methodName, params object[] parameters)
        {
            return Browser.InvokeScript(methodName, parameters);
        }

        public bool HandleException(string message, string url, string line)
        {
            return true;
        }
    }
}
