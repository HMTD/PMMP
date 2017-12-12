using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PMMP
{

    class Control
    {
        #region 字段 
        /// <summary>
        /// 服务端通信Tcp对象
        /// </summary>
        Tcp ServerTcp;
        /// <summary>
        /// 监听的IP
        /// </summary>
        public IPAddress ListenIp { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public Ports ListenPort { get; private set; }
        /// <summary>
        /// 消息处理对象
        /// </summary>
        MessageDis messageDis;
        /// <summary>
        /// 已登录未映射连接字典
        /// </summary>
        Dictionary<EndPoint, Connet> Login = new Dictionary<EndPoint, Connet>();
        /// <summary>
        /// 已验证登录，并且开启映射的连接字典
        /// </summary>
        Dictionary<EndPoint, Connet> Mmap = new Dictionary<EndPoint, Connet>();
        /// <summary>
        /// 未验证身份的连接字典
        /// </summary>
        Dictionary<EndPoint, Connet> None = new Dictionary<EndPoint, Connet>();
        /// <summary>
        /// 已开启的映射规则集合
        /// </summary>
        List<Mmaper> MmapDict = new List<Mmaper>();
        /// <summary>
        /// 映射对象
        /// </summary>
        Dictionary<EndPoint, Mmaps> MmapsDict = new Dictionary<EndPoint, Mmaps>();
        /// <summary>
        /// 服务端状态
        /// </summary>
        public bool ServerState { set; get; }
        #endregion
        #region 委托
        /// <summary>
        /// 查询映射权限委托
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public delegate string[] SeleteMmapListDelegate(string User);
        /// <summary>
        /// 查询映射方法
        /// </summary>
        public SeleteMmapListDelegate GetMmapList;
        public delegate string SelectMmapAlldelegate(string User); public SelectMmapAlldelegate SelectMmapAll;
        public delegate string SelectPassworddelegate(string User); public SelectPassworddelegate SelectPassword;
        public delegate Flow SelectFlowdelegate(string User, string IP, string Port); public SelectFlowdelegate SelectFlow;
        public delegate bool SelectMmapdelegate(string User, string IP, string Port); public SelectMmapdelegate SelectMmap;
        public delegate Speed SelectMmapSpeeddelegate(string User, string IP, string Port); public SelectMmapSpeeddelegate SelectMmapSpeed;
        public delegate void WriteSpenddlelegate(string User, string IP, string Port, string Flow); public WriteSpenddlelegate WriteSpend;
        #endregion
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iPAddress">要监听的客户端通信IP</param>
        /// <param name="Port">要监听的客户端通信端口</param>
        public Control(IPAddress iPAddress, Ports Port)
        {
            ListenIp = iPAddress;
            ListenPort = Port;
            ServerTcp = new Tcp(iPAddress, Port);                                                  // 实例化Tcp对象
            ServerTcp.Conneter += Conneter;                                                        // 绑定收到连接委托
            ServerTcp.ReceiveContext += Messager;                                                  // 绑定接收消息委托
            ServerTcp.TcpError += ConnetError;                                                     // 绑定断开连接委托
            ServerTcp.ListenStop += ListenStop;                                                    // 绑定监听停止方法
            messageDis = new MessageDis();                                                         // 实例化消息接受对象
            messageDis.CompletePack += MessagePack;                                                // 绑定包接收委托
        }
        /// <summary>
        /// 监听被停止
        /// </summary>
        private void ListenStop()
        {
            if (ServerState == true)
            {
                ServerTcp.Stop();
                ServerTcp = new Tcp(ListenIp, ListenPort);                                         // 实例化Tcp对象
                ServerTcp.Conneter += Conneter;                                                    // 绑定收到连接委托
                ServerTcp.ReceiveContext += Messager;                                              // 绑定接收消息委托
                ServerTcp.TcpError += ConnetError;                                                 // 绑定断开连接委托
                ServerTcp.ListenStop += ListenStop;                                                // 绑定监听停止方法
                messageDis = new MessageDis();                                                     // 实例化消息接受对象
                messageDis.CompletePack += MessagePack;                                            // 绑定包接收委托
            }
        }
        #region 公共方法
        /// <summary>
        /// 获取连接数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetConnets(ConnetType type)
        {
            if (type == ConnetType.Login)
            {
                return Login.Count;
            }
            else if (type == ConnetType.Mmap)
            {
                return Mmap.Count;
            }
            else if (type == ConnetType.None)
            {
                return None.Count;
            }
            else
            {
                return Login.Count + Mmap.Count + None.Count;
            }
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 收到连接
        /// </summary>
        /// <param name="endPoint">远程终结点</param>
        private void Conneter(EndPoint endPoint)
        {
            Connet NewConnet = new Connet(IPAddress.Parse(endPoint.ToString().Split(':')[0]), new Ports(int.Parse(endPoint.ToString().Split(':')[1])));// 实例化连接对象
            NewConnet.TimeOut += ConnetTimeOut;                                                    // 绑定登录超时方法
        }
        /// <summary>
        /// 收到消息方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">消息内容</param>
        /// <param name="Length">长度</param>
        private void Messager(EndPoint endPoint, byte[] Context, int Length)
        {
            messageDis.AddMessage(endPoint, Encoding.UTF32.GetString(Context, 0, Length));         // 写入消息接收
        }
        /// <summary>
        /// 连接终止
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        private void ConnetError(EndPoint endPoint)
        {
            if (Login.ContainsKey(endPoint))
                Login.Remove(endPoint);
            if (Mmap.ContainsKey(endPoint))
                Mmap.Remove(endPoint);
            messageDis.DeleteMessage(endPoint);
        }
        /// <summary>
        /// 消息处理方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        private void MessagePack(EndPoint endPoint, string[] Context)
        {
            for (int ic = 0; ic < Context.Length; ic = ic + 1)                                     // 遍历每一个包
            {
                string[] Message = Context[ic].Split(' ');                                         // 以空格分割为数组
                Connet ThisConneter = null;
                if (None.ContainsKey(endPoint))
                    ThisConneter = None[endPoint];
                if (Login.ContainsKey(endPoint))
                    ThisConneter = Login[endPoint];
                if (Mmap.ContainsKey(endPoint))
                    ThisConneter = Mmap[endPoint];
                if (Message[0] == "Login")                                                         // 鉴别为登录命令
                {
                    string User = Message[1];
                    string Pwd = Message[2];
                    if (SelectPassword(User) == Pwd && Pwd != "" && Pwd != null)            // 验证用户名密码
                    {
                        SendMessage(endPoint, "Login OK 200");                                     // 发送登录成功代码
                        Login.Add(endPoint, None[endPoint]);                                       // 在字典添加
                        Login[endPoint].ConnetType = ConnetType.Login;                             // 设为登录状态
                        None.Remove(endPoint);                                                     // 从无状态字典内移除
                    }
                    else
                    {
                        SendMessage(endPoint, "Login Error 400");                                  // 发送登录失败信息
                        ServerTcp.Close(endPoint);                                                 // 关闭连接
                    }
                }
                if (Message[0] == "Mmap")                                                          // 鉴别为请求映射命令
                {   // TCP,192.168.1.10,25565
                    if (ThisConneter.ConnetType != ConnetType.None)
                    {
                        string[] Mmap = Message[1].Split(',');                                     // 分割数据
                        for (int i = 0; i < Mmap.Length; i = i + 1)
                        {
                            if (SelectMmap(ThisConneter.User, Mmap[1], Mmap[2]))            // 遍历数据，检查权限
                            {
                                // 创建新映射对象并且实例化     用户名               IP地址                      端口                            查询映射速度        用户名        ip      端口
                                Mmaper NewMmap = new Mmaper(ThisConneter.User, IPAddress.Parse(Mmap[1]), new Ports(int.Parse(Mmap[2])), SelectMmapSpeed(ThisConneter.User, Mmap[1], Mmap[2]));
                                MmapDict.Add(NewMmap);                                             // 添加到映射规则集合
                                                                                                   // 创建映射对象并且实例化，映射规则    查询剩余流量    用户名           ip       端口
                                Mmaps mmaps = new Mmaps(NewMmap, SelectFlow(ThisConneter.User, Mmap[1], Mmap[2]));
                                MmapsDict.Add(new IPEndPoint(NewMmap.LocalhostIP, NewMmap.LocalhostPort.Port), mmaps);// 添加到映射规则字典
                                mmaps.Messages += MmapMessager;                                    // 绑定接收消息事件
                                mmaps.Conneter += MmapConneter;                                    // 绑定收到连接事件
                                mmaps.FlowEnd += MmapFlowEnd;                                      // 绑定流量耗尽事件
                                mmaps.TcpError += MmapTcpError;                                    // 绑定Tcp连接终止事件
                            }
                            else
                                SendMessage(endPoint, "Mmap Error " + Mmap[1] + ":" + Mmap[2] + "无映射权限");// 返回无权限信息
                        }
                    }
                    else
                    {
                        SendMessage(endPoint, "Mmap Error 未登录");                                // 返回无权限信息
                    }
                }
                if (Message[0] == "Select")                                                        // 鉴别为查询映射权限命令
                {
                    if (ThisConneter.ConnetType != ConnetType.None)
                    {
                        string Return = "MmapSelectReturn " + SelectMmapAll(ThisConneter.User);// 获取用户名下的所有映射规则
                        SendMessage(endPoint, Return);                                             // 发送给客户端
                    }
                    else
                    {
                        SendMessage(endPoint, "Select Error 未登录");                              // 返回无权限信息
                    }
                }
                if (Message[0] == "MmapInfo")                                                      // 鉴别为映射数据传输
                {//MmapInfo 180.1.1.5:6666->192.168.1.10:25565 XXXXXXXXXXXX(Base64)
                    if (ThisConneter.ConnetType != ConnetType.None)
                    {
                        IPAddress ClientiPAddress = IPAddress.Parse(Message[1].Split("->")[0].Split(":")[0]);// 获取客户端IP
                        Ports ClientPort = new Ports(int.Parse(Message[1].Split("->")[0].Split(":")[1]));// 获取客户端端口
                        IPAddress ServeriPAddress = IPAddress.Parse(Message[1].Split("->")[1].Split(":")[0]);// 获取服务器IP
                        Ports ServerPort = new Ports(int.Parse(Message[1].Split("->")[1].Split(":")[1]));// 获取服务器端口
                        IPEndPoint ClientiPEndPoint = new IPEndPoint(ClientiPAddress, ClientPort.Port);// 创建客户端网络终结点
                        IPEndPoint ServeriPEndPoint = new IPEndPoint(ServeriPAddress, ServerPort.Port);// 创建服务器网络终结点
                        if (MmapsDict.ContainsKey(ServeriPEndPoint))
                            MmapsDict[ServeriPEndPoint].Send(ClientiPEndPoint, Convert.FromBase64String(Message[2]));// 发送数据
                        else
                            SendMessage(endPoint, "MmapInfo Error 未找到映射");                   // 返回未找到映射
                    }
                    else
                    {
                        SendMessage(endPoint, "Select Error 未登录");                              // 返回无权限信息
                    }
                }
                if (Message[0] == "TcpClose")                                                      // 鉴别为关闭连接
                {//TcpClose 180.1.1.5:6666->192.168.1.10:25565
                    if (ThisConneter.ConnetType != ConnetType.None)
                    {
                        IPAddress ClientiPAddress = IPAddress.Parse(Message[1].Split("->")[0].Split(":")[0]);// 获取客户端IP
                        Ports ClientPort = new Ports(int.Parse(Message[1].Split("->")[0].Split(":")[1]));// 获取客户端端口
                        IPAddress ServeriPAddress = IPAddress.Parse(Message[1].Split("->")[1].Split(":")[0]);// 获取服务器IP
                        Ports ServerPort = new Ports(int.Parse(Message[1].Split("->")[1].Split(":")[1]));// 获取服务器端口
                        IPEndPoint ClientiPEndPoint = new IPEndPoint(ClientiPAddress, ClientPort.Port);// 创建客户端网络终结点
                        IPEndPoint ServeriPEndPoint = new IPEndPoint(ServeriPAddress, ServerPort.Port);// 创建服务器网络终结点
                        if (MmapsDict.ContainsKey(ServeriPEndPoint))
                            MmapsDict[ServeriPEndPoint].Close(ClientiPEndPoint);                   // 关闭连接
                        else
                            SendMessage(endPoint, "TcpClose Error 未找到映射");                    // 返回未找到映射
                    }
                    else
                    {
                        SendMessage(endPoint, "Select Error 未登录");                              // 返回无权限信息
                    }
                }
                if (Message[0] == "MmapStop")                                                      // 鉴别为停止映射
                {//MmapStop 192.168.1.10:25565
                    if (ThisConneter.ConnetType != ConnetType.None)
                    {
                        IPAddress ServeriPAddress = IPAddress.Parse(Message[1].Split("->")[1].Split(":")[0]);// 获取服务器IP
                        Ports ServerPort = new Ports(int.Parse(Message[1].Split("->")[1].Split(":")[1]));// 获取服务器端口
                        IPEndPoint ServeriPEndPoint = new IPEndPoint(ServeriPAddress, ServerPort.Port);// 创建服务器网络终结点
                        if (MmapsDict.ContainsKey(ServeriPEndPoint))
                        {
                            Flow flow = MmapsDict[ServeriPEndPoint].Stop();                        // 映射并且获取剩余流量
                            double MB = flow.MB;                                                   // 获取MB数值
                            if (flow.IsInfinity)
                            {
                                WriteSpend(ThisConneter.User, ServeriPAddress.ToString(), ServerPort.Port.ToString(), "∞");
                            }
                            else
                            {
                                WriteSpend(ThisConneter.User, ServeriPAddress.ToString(), ServerPort.Port.ToString(), MB.ToString());
                            }
                        }
                        else
                            SendMessage(endPoint, "TcpClose Error 未找到映射");                    // 返回未找到映射
                    }
                    else
                    {
                        SendMessage(endPoint, "Select Error 未登录");                              // 返回无权限信息
                    }
                }
            }
        }
        /// <summary>
        /// 发送消息方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">内容</param>
        private void SendMessage(EndPoint endPoint, string Context)
        {
            string Base64Context = Convert.ToBase64String(Encoding.UTF8.GetBytes(Context));        // 把要发送的字符串专为字节数组再转为base64代码
            byte[] SendMessage = Encoding.UTF8.GetBytes("<MappPackStart>{" + Base64Context + "}<MappPackEnd>");// 把base64字符串装入包内
            try
            {
                ServerTcp.Send(endPoint, SendMessage);                                             // 发送消息
            }
            catch { ServerTcp.Close(endPoint); }
        }
        /// <summary>
        /// 登录超时方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        private void ConnetTimeOut(EndPoint endPoint)
        {
            SendMessage(endPoint, "Mppm Login TimeOut");                                           // 发送超时消息
            ServerTcp.Close(endPoint);                                                             // 关闭tcp连接
        }
        #endregion
        #region 映射对象绑定事件
        /// <summary>
        /// 收到连接事件
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="mmaper">映射规则</param>
        private void MmapConneter(EndPoint endPoint, Mmaper mmaper)
        {//NewConnet 180.1.1.5:6666->192.168.1.10:25565
            // 要发送的数据包字符串 指令        客户端IP:端口                监听IP                        :          端口  
            string Messages = "NewConnet " + endPoint.ToString() + "->" + mmaper.LocalhostIP.ToString() + ":" + mmaper.LocalhostPort.Port;
            SendMessage(new IPEndPoint(mmaper.LocalhostIP, mmaper.LocalhostPort.Port), Messages);
        }
        /// <summary>
        /// 收到消息方法
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="Context">消息内容</param>
        /// <param name="Length">长度</param>
        private void MmapMessager(EndPoint endPoint, byte[] Context, int Length, Mmaper mmaper)
        {//MmapInfo 180.1.1.5:6666->192.168.1.10:25565 XXXXXXXXXXXX(Base64)
            // 要发送的数据包字符串 指令       客户端IP:端口                监听IP                        :          端口                                转为Base64的数据
            string Messages = "MmapInfo " + endPoint.ToString() + "->" + mmaper.LocalhostIP.ToString() + ":" + mmaper.LocalhostPort.Port + " " + Convert.ToBase64String(Context);
            SendMessage(new IPEndPoint(mmaper.LocalhostIP, mmaper.LocalhostPort.Port), Messages);
        }
        /// <summary>
        /// 客户端断开连接事件
        /// </summary>
        /// <param name="endPoint">远程网络终结点</param>
        /// <param name="mmaper">映射规则</param>
        private void MmapTcpError(EndPoint endPoint, Mmaper mmaper)
        {//TcpError 180.1.1.5:6666->192.168.1.10:25565
            // 要发送的数据包字符串 指令       客户端IP:端口                监听IP                        :          端口  
            string Messages = "TcpError " + endPoint.ToString() + "->" + mmaper.LocalhostIP.ToString() + ":" + mmaper.LocalhostPort.Port;
            SendMessage(new IPEndPoint(mmaper.LocalhostIP, mmaper.LocalhostPort.Port), Messages);
        }
        /// <summary>
        /// 映射流量耗尽事件
        /// </summary>
        /// <param name="mmaper">映射规则</param>
        private void MmapFlowEnd(Mmaper mmaper)
        {//MmapFlowEnd 192.168.1.10:25565
            string Messages = "MmapFlowEnd " + mmaper.LocalhostIP.ToString() + ":" + mmaper.LocalhostPort.Port;
            SendMessage(new IPEndPoint(mmaper.LocalhostIP, mmaper.LocalhostPort.Port), Messages);
        }
        #endregion
    }
}