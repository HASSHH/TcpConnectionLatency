using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;

namespace TpcConnectionLatency
{
    class ApplicationTcpConnections
    {
        public readonly List<TcpConnectionInfo> Connections = new();

        public void RefreshList(string appNameFilter)
        {
            Connections.Clear();
            var tcpTable = GetTcpTable(true);
            var ownerTable = GetExtendedTcpTable<MIB_TCPTABLE_OWNER_MODULE>(TCP_TABLE_CLASS.TCP_TABLE_OWNER_MODULE_ALL, Ws2_32.ADDRESS_FAMILY.AF_INET, true);
            var ownerNames = ownerTable.Select(r => {
                try 
                {
                    return GetOwnerModuleFromTcpEntry(r);
                } 
                catch 
                { 
                    return new TCPIP_OWNER_MODULE_BASIC_INFO { pModuleName = string.Empty }; 
                } 
            }).ToList();
            for (int i = 0; i < ownerNames.Count; ++i)
                if ((string.IsNullOrWhiteSpace(appNameFilter) || ownerNames[i].pModuleName.Contains(appNameFilter)) && ownerTable.table[i].dwRemoteAddr.S_addr != 16777343)
                {
                    Win32Error returnCode = GetPerTcpConnectionEStats(tcpTable.table[i], TCP_ESTATS_TYPE.TcpConnectionEstatsPath, out object rw, out object ros, out object rod);
                    if (returnCode == Win32Error.NO_ERROR)
                        Connections.Add(new TcpConnectionInfo((TCP_ESTATS_PATH_ROD_v0)rod, ownerTable.table[i], tcpTable.table[i], ownerNames[i]));
                }
        }

        public Win32Error Update(TcpConnectionInfo connectionInfo)
        {
            Win32Error returnCode = GetPerTcpConnectionEStats(connectionInfo.GetTcpRow(), TCP_ESTATS_TYPE.TcpConnectionEstatsPath, out object rw, out object ros, out object rod);
            if (returnCode == Win32Error.NO_ERROR)
                connectionInfo.SetStats((TCP_ESTATS_PATH_ROD_v0)rod);
            return returnCode;
        }
    }
}
