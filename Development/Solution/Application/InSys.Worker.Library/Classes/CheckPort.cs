using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace InSys.Worker.Library.Classes
{
    public class CheckPort
    {
        public static bool IsAvailable(int port)
        {
            bool isAvailable = true;
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }
                return isAvailable;
            }
            catch(Exception ex)
            {
                throw new Exception($"Something went wrong when checking port {port.ToString()} if available.");
            }
        }
    }
}
