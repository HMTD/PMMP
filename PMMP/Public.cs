using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PMMP
{
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
    class Mmaper
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="localhostIP">服务端IP地址</param>
        /// <param name="localhostPort">映射端口</param>
        public Mmaper(string User, IPAddress localhostIP, Ports localhostPort)
        {

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
        public long Byte { private set; get; }
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
