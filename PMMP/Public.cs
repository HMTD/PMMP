using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PMMP
{
    /// <summary>
    /// 连接对象类
    /// </summary>
    class Connet
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="IP">连接的IP地址</param>
        /// <param name="Port">连接的端口</param>
        public Connet(IPAddress IP, Ports Port)
        {
            ConnetIP = IP;
            ConnetPort = Port;
            ConnetType = ConnetType.None;
            CoutDownSecond = 60;
            IsWait = true;
            Task CoutDownOneMin = new Task(new Action(() =>
            {
                for (int i = 0; i < 60; i = i + 1)
                {
                    CoutDownSecond = CoutDownSecond - 1;
                    Thread.Sleep(1000);
                }
                if (ConnetType != ConnetType.None)
                {
                    IsLegitimate = true;
                    IsWait = false;
                }
                else
                {
                    IsLegitimate = false;
                    IsWait = false;
                    TimeOut(new IPEndPoint(ConnetIP, ConnetPort.Port));
                }
            }));
            CoutDownOneMin.Start();
        }
        /// <summary>
        /// 连接IP地址
        /// </summary>
        public IPAddress ConnetIP { private set; get; }
        /// <summary>
        /// 连接端口
        /// </summary>
        public Ports ConnetPort { private set; get; }
        /// <summary>
        /// 连接模式
        /// </summary>
        public ConnetType ConnetType { set; get; }
        /// <summary>
        /// 连接是否合法
        /// </summary>
        public bool IsLegitimate { private set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string User { set; get; }
        /// <summary>
        /// 是否等待中
        /// </summary>
        public bool IsWait { private set; get; }
        /// <summary>
        /// 倒计时剩余秒数
        /// </summary>
        public int CoutDownSecond { private set; get; }
        public delegate void TimeOutDelegate(EndPoint endPoint);
        public TimeOutDelegate TimeOut;
    }
    /// <summary>
    /// 端口对象类
    /// </summary>
    class Ports
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">端口0-65535</param>
        public Ports(int port)
        {
            if (port >= 0 && port <= 65535)
            {
                Port = port;
            }
            else
            {
                throw new Exception("端口值错误，小于零或者大于65535");
            }
        }
        /// <summary>
        /// 获取端口
        /// </summary>
        public int Port { get; private set; }
    }
    /// <summary>
    /// 映射规则类
    /// </summary>
    class Mmaper
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="localhostIP">服务端IP地址</param>
        /// <param name="localhostPort">映射端口</param>
        /// <param name="MMAPSpend">映射限制速度</param>
        public Mmaper(string User, IPAddress localhostIP, Ports localhostPort, Speed MMAPSpend)
        {
            this.User = User;
            LocalhostIP = localhostIP;
            LocalhostPort = localhostPort;
            MmapSpend = MMAPSpend;
        }
        /// <summary>
        /// 用户名
        /// </summary>
        public string User { private set; get; }
        /// <summary>
        /// 服务端IP地址
        /// </summary>
        public IPAddress LocalhostIP { private set; get; }
        /// <summary>
        /// 映射端口
        /// </summary>
        public Ports LocalhostPort { private set; get; }
        /// <summary>
        /// 映射速度
        /// </summary>
        public Speed MmapSpend { private set; get; }
    }
    class Speed
    {
        public Speed(long bytes)
        {
            Byte = bytes;
            Bit = bytes * 8;
        }
        /// <summary>
        /// 字节速度
        /// </summary>
        public long Byte { private set; get; }
        /// <summary>
        /// 比特速度
        /// </summary>
        public long Bit { private set; get; }
        /// <summary>
        /// 返回KB/S速度数值
        /// </summary>
        /// <returns></returns>
        public long GetKB_S()
        {
            return Byte / 1024;
        }
        /// <summary>
        /// 返回Kbps速度数值
        /// </summary>
        /// <returns></returns>
        public long GetKbps()
        {
            return (Byte / 1024) * 8;
        }
        /// <summary>
        /// 返回MB/S速度数值
        /// </summary>
        /// <returns></returns>
        public long GetMB_S()
        {
            return (Byte / 1024) / 1024;
        }
        /// <summary>
        /// 返回Mbps速度数值
        /// </summary>
        /// <returns></returns>
        public long GetMbps()
        {
            return ((Byte / 1024) / 1024) * 8;
        }
    }
    class Flow
    {
        public Flow(double Bytes)
        {
            B = Bytes;
            KB = Bytes / 1024;
            MB = Bytes / 1024 / 1024;
            GB = Bytes / 1024 / 1024 / 1024;
        }
        public void Sub(double bytes)
        {
            B = B - bytes;
            KB = B / 1024;
            MB = KB / 1024;
            GB = KB / 1024;
        }
        public void Add(double bytes)
        {
            B = B + bytes;
            KB = B / 1024;
            MB = KB / 1024;
            GB = KB / 1024;
        }
        public double B { get; set; }
        public double KB { set; get; }
        public double MB { set; get; }
        public double GB { set; get; }
        public bool IsInfinity { set; get; }
    }
    enum SpeedType
    {
        Byte,
        Bit
    }
    enum ConnetType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        None,
        /// <summary>
        /// 已登录未映射
        /// </summary>
        Login,
        /// <summary>
        /// 已开启映射
        /// </summary>
        Mmap,
        All
    }
}
