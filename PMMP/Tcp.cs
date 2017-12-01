using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PMMP
{
    class Tcp
    {
        TcpListener TcpL;
        Socket ListenSocket;
        Task AcceptThreadTask;
        public Dictionary<EndPoint, Socket> SocketDict = new Dictionary<EndPoint, Socket>();
        List<Socket> SocketList = new List<Socket>();
        public delegate void TcpErrorDelegate(EndPoint endPoint);
        public TcpErrorDelegate TcpError;
        public delegate void ReceiveContextDelegate(EndPoint endPoint, byte[] Context, int Length);
        public ReceiveContextDelegate ReceiveContext;
        public delegate void ConneterDelegate(EndPoint endPoint);
        public ConneterDelegate Conneter;
        public Tcp(IPAddress IP, Ports Port)
        {
            TcpL = new TcpListener(IP, Port.Port);
            TcpL.Start();
            ListenSocket = TcpL.Server;
            AcceptThreadTask = new Task(new Action(WhileAccept));
            AcceptThreadTask.Start();
        }
        public void Stop()
        {
            TcpL.Server.Close();
            TcpL.Server.Dispose();
            TcpL.Stop();
            TcpL = null;
            for (int i = SocketList.Count - 1; i >= 0; i = i + 1)
            {
                SocketList[i].Close();
                SocketList[i].Dispose();
                SocketList[i] = null;
                SocketList.RemoveAt(i);
            }
            SocketList = null;
            GC.Collect();
        }
        public void Send(EndPoint endPoint, byte[] Context)
        {
            SocketDict[endPoint].Send(Context);
        }
        public void Close(EndPoint endPoint)
        {
            SocketDict[endPoint].Close();
        }
        private void WhileAccept()
        {
            while (true)
            {
                bool IsHaveConnet = true;
                try
                {
                    IsHaveConnet = TcpL.Pending();
                }
                catch { goto cc; }
                if (IsHaveConnet)
                {
                    Socket NewConnet = TcpL.AcceptSocket();
                    Conneter(NewConnet.LocalEndPoint);
                    Task ReceiveTask = new Task(new Action(() => ReceiveMessage(NewConnet)));
                    ReceiveTask.Start();
                    SocketList.Add(NewConnet);
                    SocketDict.Add(NewConnet.RemoteEndPoint, NewConnet);
                }
            }
            cc: string a = "";
            a = a + "";
        }
        private void ReceiveMessage(Socket ConnetSocket)
        {
            byte[] Buffer = new byte[1500];
            while (ConnetSocket.Connected)
            {
                int Length = -1;
                try
                {
                    Length = ConnetSocket.Receive(Buffer);
                }
                catch
                {
                    goto cc;
                }
                ReceiveContext(ConnetSocket.RemoteEndPoint, Buffer, Length);
            }
            cc: ConnetSocket.Close();
            ConnetSocket.Dispose();
            GC.Collect();
            TcpError(ConnetSocket.RemoteEndPoint);
        }
    }
}
