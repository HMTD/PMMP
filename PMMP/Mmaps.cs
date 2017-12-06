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
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="mmaper">映射规则</param>
        /// <param name="flow">剩余流量</param>
        public Mmaps(Mmaper mmaper, Flow flow)
        {
            IP = mmaper.LocalhostIP;
            Port = mmaper.LocalhostPort;
            speed = mmaper.MmapSpend;
            Flow = flow;
            ThisMmaper = mmaper;
        }
        /// <summary>
        /// 开启监听
        /// </summary>
        public void Start()
        {
            ServerTcp = new Tcp(IP, Port);                                                         // 实例化Tcp类对象
            ServerTcp.Conneter += Connet;                                                          // 绑定收到连接请求方法
            ServerTcp.ReceiveContext += Mesage;                                                    // 绑定收到消息方法
            ServerTcp.TcpError += TcpClose;                                                        // 绑定Tcp连接关闭方法
            ServerTcp.ListenStop += ListenStop;                                                    // 绑定监听关闭事件
        }
        #region 属性
        /// <summary>
        /// 监听IP
        /// </summary>
        public IPAddress IP { set; get; }
        /// <summary>
        /// 监听端口
        /// </summary>
        public Ports Port { set; get; }
        /// <summary>
        /// 映射限速
        /// </summary>
        public Speed speed { set; get; }
        /// <summary>
        /// 剩余流量
        /// </summary>
        public Flow Flow { set; get; }
        /// <summary>
        /// 映射状态
        /// </summary>
        public bool MmapState { get; private set; }
        /// <summary>
        /// 映射规则
        /// </summary>
        public Mmaper ThisMmaper { get; private set; }
        #endregion
        #region 字段
        /// <summary>
        /// Tcp连接字典
        /// </summary>
        Dictionary<EndPoint, Flow> TcpConnetDict = new Dictionary<EndPoint, Flow>();
        /// <summary>
        /// Tcp对象
        /// </summary>
        Tcp ServerTcp;
        #endregion
        #region 委托
        /// <summary>
        /// 收到连接委托
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        public delegate void ConneterDelegate(EndPoint endPoint, Mmaper mmaper);
        /// <summary>
        /// 收到连接委托
        /// </summary>
        public ConneterDelegate Conneter;
        /// <summary>
        /// 收到消息委托
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Comtext">信息</param>
        /// <param name="Legth">长度</param>
        public delegate void MessagesDelegate(EndPoint endPoint, byte[] Comtext, int Legth, Mmaper mmaper);
        /// <summary>
        /// 收到消息委托
        /// </summary>
        public MessagesDelegate Messages;
        /// <summary>
        /// Tcp连接断开委托
        /// </summary>
        /// <param name="endPoint">网络终结点</param>
        public delegate void TcpErrorDelegate(EndPoint endPoint, Mmaper mmaper);
        /// <summary>
        /// Tcp连接断开委托
        /// </summary>
        public TcpErrorDelegate TcpError;
        /// <summary>
        /// 流量耗尽委托
        /// </summary>
        public delegate void FlowEndDelegate(Mmaper mmaper);
        /// <summary>
        /// 流量耗尽委托
        /// </summary>
        public FlowEndDelegate FlowEnd;
        #endregion
        #region 公共方法
        /// <summary>
        /// 发送信息的方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        public void Send(EndPoint endPoint, byte[] Context)// 计费
        {
            Flow.Sub(Context.Length);                                                              // 计费操作
            if (Flow.B <= 0)                                                                       // 检查流量是否耗光
            {
                FlowEnd(ThisMmaper);                                                                         // 如果耗光就执行流量耗尽委托方法
            }
            ServerTcp.Send(endPoint, Context);                                                     // 发送数据
        }
        /// <summary>
        /// 关闭一个连接
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        public void Close(EndPoint endPoint)
        {
            ServerTcp.Close(endPoint);                                                             // 根据远程网络终结点关闭一个连接
        }
        /// <summary>
        /// 停止映射
        /// </summary>
        /// <returns></returns>
        public Flow Stop()
        {
            MmapState = false;                                                                     // 标记映射状态为停止状态
            ServerTcp.Stop();                                                                      // 关闭端口监听
            return Flow;                                                                           // 返回剩余流量
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 收到连接方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        private void Connet(EndPoint endPoint)
        {
            TcpConnetDict.Add(endPoint, new Flow(0));                                              // 添加到映射
            Connet(endPoint);                                                                      // 执行接收连接方法
        }
        /// <summary>
        /// 收到消息方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        /// <param name="Length">长度</param>
        private void Mesage(EndPoint endPoint, byte[] Context, int Length)// 计费
        {
            Flow.Sub((double)Length);                                                              // 计费操作
            if (Flow.B <= 0)                                                                       // 判断是否流量少于0
            {
                FlowEnd(ThisMmaper);                                                                         // 如果是就执行流量耗尽委托方法
            }
            Messages(endPoint, Context, Length, ThisMmaper);                                                   // 收到消息委托方法
        }
        /// <summary>
        /// Tcp连接终止方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        private void TcpClose(EndPoint endPoint)
        {
            TcpError(endPoint, ThisMmaper);                                                                    // 执行连接中断委托方法
        }
        /// <summary>
        /// Tcp监听终止事件
        /// </summary>
        private void ListenStop()
        {
            if (MmapState == true)
            {
                ServerTcp.Stop();
                ServerTcp = new Tcp(IP, Port);                                                     // 实例化Tcp类对象
                ServerTcp.Conneter += Connet;                                                      // 绑定收到连接请求方法
                ServerTcp.ReceiveContext += Mesage;                                                // 绑定收到消息方法
                ServerTcp.ReceiveContext += Mesage;                                                // 绑定收到消息方法
                ServerTcp.TcpError += TcpClose;                                                    // 绑定Tcp连接关闭方法
            }
        }
        #endregion
    }
}
