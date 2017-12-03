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
        TcpListener TcpL;                                                                          // TcpListener对象
        Socket ListenSocket;                                                                       // 监听套接字
        Task AcceptThreadTask;                                                                     // 接受连接线程
        /// <summary>
        /// 储存连接套接字的字典
        /// </summary>
        public Dictionary<EndPoint, Socket> SocketDict = new Dictionary<EndPoint, Socket>();
        List<Socket> SocketList = new List<Socket>();                                              // 套接字数组
        public delegate void TcpErrorDelegate(EndPoint endPoint);
        /// <summary>
        /// Tcp连接断开委托
        /// </summary>
        public TcpErrorDelegate TcpError;
        public delegate void ReceiveContextDelegate(EndPoint endPoint, byte[] Context, int Length);
        /// <summary>
        /// 收到消息委托
        /// </summary>
        public ReceiveContextDelegate ReceiveContext;
        public delegate void ConneterDelegate(EndPoint endPoint);
        /// <summary>
        /// 新连接委托
        /// </summary>
        public ConneterDelegate Conneter;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="IP">监听地址</param>
        /// <param name="Port">监听IP</param>
        public Tcp(IPAddress IP, Ports Port)
        {
            TcpL = new TcpListener(IP, Port.Port);                                                 // new一个TcpListener对象
            TcpL.Start();                                                                          // 开始监听
            ListenSocket = TcpL.Server;                                                            // 赋值监听套接字
            AcceptThreadTask = new Task(new Action(WhileAccept));                                  // 创建接受连接线程
            AcceptThreadTask.Start();                                                              // 开始接受连接线程
        }
        /// <summary>
        /// 停止TCP方法
        /// </summary>
        public void Stop()
        {
            TcpL.Stop();                                                                           // 关闭侦听程序
            TcpL = null;                                                                           // 赋值为空
            for (int i = SocketList.Count - 1; i >= 0; i = i + 1)                                  // 遍历连接socket
            {
                SocketList[i].Close();                                                             // 关闭连接
                SocketList[i].Dispose();                                                           // 释放资源
                SocketList[i] = null;                                                              // 赋值为空
            }
            SocketList.RemoveRange(0, SocketList.Count - 1);
            SocketList = null;
            GC.Collect();                                                                          // 清理内存
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        public void Send(EndPoint endPoint, byte[] Context)
        {
            SocketDict[endPoint].Send(Context);
        }
        /// <summary>
        /// 关闭一个连接
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        public void Close(EndPoint endPoint)
        {
            SocketDict[endPoint].Close();
        }
        /// <summary>
        /// 接收数据
        /// </summary>
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
