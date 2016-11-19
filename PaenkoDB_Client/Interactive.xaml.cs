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
        public Interactive()
        {
            InitializeComponent();
            Map = GoogleMapWrapper.Create(this);
            Map.ApiReady += () => DrawNodes();
        }

        void DrawNodes()
        {
            MarkerOptions mo = new MarkerOptions() { Icon = @"http://i.imgur.com/phDUsCN.png", Clickable = true, DraggingEnabled = false, Flat = false, Optimized = true, RaiseOnDrag = true };
            foreach (PaenkoNode pn in Init.Peers)
            {
                var Marker = Map.AddMarker(new GeographicLocation(pn.NodeLocation.lat, pn.NodeLocation.lon), mo);
                Marker.Click += (im ,gl) =>
                {
                    Main m = new Main();
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
