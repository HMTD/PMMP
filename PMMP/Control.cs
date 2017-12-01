using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PMMP
{
    class Control
    {
        Tcp ServerTcp;
        Dictionary<string, Connet> Login = new Dictionary<string, Connet>();
        public Control(IPAddress iPAddress, Ports Port)
        {
            ServerTcp = new Tcp(iPAddress, Port);
            ServerTcp.Conneter += Conneter;
        }
        private void Conneter(EndPoint endPoint)
        {
            Connet NewConnet = new Connet(endPoint.a);
        }
    }
}