using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace PMMP
{
    class Tcp
    {
        #region 字段
        TcpListener TcpL;                                                                          // TcpListener对象
        Socket ListenSocket;                                                                       // 监听套接字
        Task AcceptThreadTask;                                                                     // 接受连接线程
        List<Socket> SocketList = new List<Socket>();                                              // 套接字数组
        #endregion
        #region 字典
        /// <summary>
        /// 储存连接套接字的字典
        /// </summary>
        public Dictionary<EndPoint, Socket> SocketDict = new Dictionary<EndPoint, Socket>();
        #endregion
        #region 委托
        /// <summary>
        /// Tcp连接中断委托
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        public delegate void TcpErrorDelegate(EndPoint endPoint);
        /// <summary>
        /// Tcp连接断开委托
        /// </summary>
        public TcpErrorDelegate TcpError;
        /// <summary>
        /// 收到消息委托
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        /// <param name="Length">长度</param>
        public delegate void ReceiveContextDelegate(EndPoint endPoint, byte[] Context, int Length);
        /// <summary>
        /// 收到消息委托
        /// </summary>
        public ReceiveContextDelegate ReceiveContext;
        /// <summary>
        /// 收到连接委托
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        public delegate void ConneterDelegate(EndPoint endPoint);
        /// <summary>
        /// 新连接委托
        /// </summary>
        public ConneterDelegate Conneter;
        /// <summary>
        /// 接收终止
        /// </summary>
        public delegate void ListenStopDelegate();
        /// <summary>
        /// 接受终止
        /// </summary>
        public ListenStopDelegate ListenStop;
        #endregion
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
        #region 公共方法
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
        #endregion
        #region 私有方法
        /// <summary>
        /// 接收连接
        /// </summary>
        private void WhileAccept()
        {
            while (true)
            {
                bool IsHaveConnet = true;
                try
                {
                    IsHaveConnet = TcpL.Pending();                                                 // 是否有等待的连接请求
                }
                catch { goto cc; }
                if (IsHaveConnet)                                                                  // 如果有
                {
                    Socket NewConnet;
                    try
                    {
                        NewConnet = TcpL.AcceptSocket();                                           // 获取连接套接字
                    }
                    catch { goto cc; }
                    Conneter(NewConnet.RemoteEndPoint);                                            // 获取远程网络终结点
                    Task ReceiveTask = new Task(new Action(() => ReceiveMessage(NewConnet)));      // 创立消息接收线程
                    ReceiveTask.Start();                                                           // 启动线程
                    SocketList.Add(NewConnet);                                                     // 添加到数组
                    SocketDict.Add(NewConnet.RemoteEndPoint, NewConnet);                           // 添加到字典
                }
            }
            cc: Thread.Sleep(1000);                                                                // 跳出循环
            ListenStop();                                                                          // 调用接受终止方法
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ConnetSocket">Tcp连接的Socket</param>
        private void ReceiveMessage(Socket ConnetSocket)
        {
            byte[] Buffer = new byte[1500];                                                        // 创建一个缓冲区
            while (ConnetSocket.Connected)                                                         // 确认是否还连接
            {
                Buffer = null;                                                                     // 清空缓冲区
                Buffer = new byte[1500];                                                           // 重新赋值
                int Length = -1;                                                                   // 设置接收大小变量
                try
                {
                    Length = ConnetSocket.Receive(Buffer);                                         // 接收数据并返回数据长度
                }
                catch
                {
                    goto cc;                                                                       // 出错就跳出循环
                }
                ReceiveContext(ConnetSocket.RemoteEndPoint, Buffer, Length);                       // 用委托传出数据
            }
            cc: ConnetSocket.Close();                                                              // 跳出循环，关闭套接字
            ConnetSocket.Dispose();                                                                // 释放所有资源
            GC.Collect();                                                                          // 清理内存
            TcpError(ConnetSocket.RemoteEndPoint);                                                 // 通过委托，发出连接中断信息
        }
        #endregion


    }
}
