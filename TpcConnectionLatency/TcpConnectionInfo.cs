using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;

namespace TpcConnectionLatency
{
    class TcpConnectionInfo
    {
        private TCP_ESTATS_PATH_ROD_v0 stats;
        private MIB_TCPROW_OWNER_MODULE ownerRow;
        private MIB_TCPROW tcpRow;
        private TCPIP_OWNER_MODULE_BASIC_INFO ownerInfo;

        public TcpConnectionInfo(TCP_ESTATS_PATH_ROD_v0 stats, MIB_TCPROW_OWNER_MODULE ownerRow, MIB_TCPROW tcpRow, TCPIP_OWNER_MODULE_BASIC_INFO ownerInfo)
        {
            this.stats = stats;
            this.ownerRow = ownerRow;
            this.tcpRow = tcpRow;
            this.ownerInfo = ownerInfo;
        }

        public uint Current => stats.SampleRtt;
        public uint Smoothed => stats.SmoothedRtt;
        public float Avg => (float)stats.SumRtt / stats.CountRtt;
        public uint Min => stats.MinRtt;
        public uint Max => stats.MaxRtt;
        public string RemoteIpv4Addr => ownerRow.dwRemoteAddr.ToString();
        public string Name => ownerInfo.pModuleName;
        public string Path => ownerInfo.pModulePath;

        public MIB_TCPROW GetTcpRow() => tcpRow;
        public void SetStats(TCP_ESTATS_PATH_ROD_v0 stats)
        {
            this.stats = stats;
        }
    }
}
