using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace InSys.Helper
{
    public class IpAddress
    {
        public IpAddress() { }
        public static string GetIP()
        {
            string hostName = Dns.GetHostName();
            if (Dns.GetHostEntry(hostName).AddressList.Length == 0) return null;
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            return myIP;
        }
    }
}
