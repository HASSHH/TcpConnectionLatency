using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Vanara.PInvoke;
using OpenTK;
using TcpConnectionLatency.Graphics;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace TcpConnectionLatency
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl glControl;
        private Scene scene;
        private ApplicationTcpConnections atc;
        private System.Windows.Forms.Timer timer;

        public MainWindow()
        {
            InitializeComponent();
            atc = new ApplicationTcpConnections();
            InitializeTimer();
            InitializeGlComponent();
            scene = new Scene(glControl);
        }

        private void InitializeTimer()
        {
            if(timer == null)
            {
                timer = new System.Windows.Forms.Timer { Interval = 1000 };
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void InitializeGlComponent()
        {
            glControl = new GLControl();
            Toolkit.Init();
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            WFHost.Child = glControl;
            glControl.CreateControl();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Refresh();
            glControl?.Invalidate();
        }

        private void Refresh()
        {
            MainWindowModel model = DataContext as MainWindowModel;
            atc.RefreshList(model.Filter);
            if (model.SelectedConnection != null)
            {
                //change highlighted conn
                model.HighlightedConnection = model.SelectedConnection;
                scene.Values.Clear();
                model.HighlightedGeolocation = string.Empty;
                SetGeolocation(model.HighlightedConnection);
            }
            model.Connections.Clear();
            foreach (TcpConnectionInfo tci in atc.Connections)
                model.Connections.Add(tci);
            if(model.HighlightedConnection != null)
            {
                Win32Error result = atc.Update(model.HighlightedConnection);
                if (result.Failed)
                {
                    model.HighlightedConnection = null;
                    scene.Values.Clear();
                }
                else
                {
                    model.HighlightedConnection = model.HighlightedConnection;//force bindings update
                    scene.Values.Add(model.HighlightedConnection.Current);
                    if (scene.Values.Count > 60)
                        scene.Values.RemoveAt(0);
                    uint maxVal = scene.Values.Max();
                    maxVal = Math.Max(maxVal, 100);
                    scene.Height = maxVal;
                    model.GraphHeight = maxVal;
                }
            }
        }

        private async void SetGeolocation(TcpConnectionInfo connection)
        {
            MainWindowModel model = DataContext as MainWindowModel;
            GeolocationResponse geolocation = await GetLocationFromIp(connection.RemoteIpv4Addr);
            if(geolocation != null)
            {
                if(model.HighlightedConnection == connection)//check if highlighted con. is still the same one
                {
                    model.HighlightedGeolocation = $"{geolocation.country}({geolocation.countryCode}), {geolocation.regionName}";
                }
            }
        }

        private async Task<GeolocationResponse> GetLocationFromIp(string ipAddr)
        {
            try
            {
                string apiBasePath = "http://ip-api.com/json/";
                string param = "?fields=11";
                HttpClient client = new HttpClient();
                return await client.GetFromJsonAsync<GeolocationResponse>(apiBasePath+ipAddr+param);
            }
            catch
            {
                return null;
            }
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            scene?.Resize();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            scene?.Paint();
        }
    }
}
