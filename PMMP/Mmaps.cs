using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PMMP
{
    class Mmaps
    {
        public Mmaps(Mmaper mmaper, Flow flow)
        {
            IP = mmaper.LocalhostIP;
            Port = mmaper.LocalhostPort;
            speed = mmaper.MmapSpend;
            Flow = flow;
        }

        public void Start()
        {
            ServerTcp = new Tcp(IP, Port);

        }
        public IPAddress IP { set; get; }
        public Ports Port { set; get; }
        public Speed speed { set; get; }
        public Flow Flow { set; get; }
        Dictionary<EndPoint, Flow> TcpConnetDict = new Dictionary<EndPoint, Flow>();
        Tcp ServerTcp;
        public delegate void ConneterDelegate(EndPoint endPoint);
        public ConneterDelegate Conneter;
        public delegate void MessagesDelegate(EndPoint endPoint, byte[] Comtext, int Legth);
        public MessagesDelegate Messages;
        public delegate void TcpErrorDelegate(EndPoint endPoint);
        public TcpErrorDelegate TcpError;
        public void Send()
        {

        }
        public void Close()
        {

        }
        public void Stop()
        {
            for (int i = 0; i < TcpConnetDict.Count; i = i + 1)
            {

            }
        }
        private void Connet(EndPoint endPoint)
        {
            TcpConnetDict.Add(endPoint, new Flow(0));
            Connet(endPoint);
        }
        private void Mesage(EndPoint endPoint, byte[] Context, int Length)
        {
            Flow.Add((double)Length);
            Messages(endPoint, Context, Length);
        }
    }
}
