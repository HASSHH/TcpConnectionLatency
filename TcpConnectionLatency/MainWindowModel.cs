using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnectionLatency
{
    class MainWindowModel : INotifyPropertyChanged
    {
        private ObservableCollection<TcpConnectionInfo> connections = new();
        private TcpConnectionInfo selectedConnection;
        private TcpConnectionInfo highlightedConnection;
        private uint graphHeight = 100;
        private string filter;
        private string highlightedGeolocation;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TcpConnectionInfo> Connections { get => connections; set { connections = value; OnPropertyChanged("Connections"); } }
        public TcpConnectionInfo SelectedConnection { get => selectedConnection; set { selectedConnection = value; OnPropertyChanged("SelectedConnection"); } }
        public TcpConnectionInfo HighlightedConnection { get => highlightedConnection; set { highlightedConnection = value; OnPropertyChanged("HighlightedConnection"); } }
        public string Filter { get => filter; set { filter = value; OnPropertyChanged("Filter"); } }
        public uint GraphHeight { get => graphHeight; set { graphHeight = value; OnPropertyChanged("GraphHeight"); } }
        public string HighlightedGeolocation { get => highlightedGeolocation; set { highlightedGeolocation = value; OnPropertyChanged("HighlightedGeolocation"); } }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
