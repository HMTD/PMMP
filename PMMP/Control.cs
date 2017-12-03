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
        /// <summary>
        /// 服务端通信Tcp对象
        /// </summary>
        Tcp ServerTcp;
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
        /// 已开启的映射规则字典
        /// </summary>
        Dictionary<EndPoint, Mmaper> MmapDict = new Dictionary<EndPoint, Mmaper>();
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
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iPAddress">要监听的客户端通信IP</param>
        /// <param name="Port">要监听的客户端通信端口</param>
        public Control(IPAddress iPAddress, Ports Port)
        {
            ServerTcp = new Tcp(iPAddress, Port);                                                  // 实例化Tcp对象
            ServerTcp.Conneter += Conneter;                                                        // 绑定收到连接委托
            ServerTcp.ReceiveContext += Messager;                                                  // 绑定接收消息委托
            ServerTcp.TcpError += ConnetError;                                                     // 绑定断开连接委托
            messageDis = new MessageDis();                                                         // 实例化消息接受对象
            messageDis.CompletePack += MessagePack;                                                // 绑定包接收委托
        }
        private void Conneter(EndPoint endPoint)
        {
            Connet NewConnet = new Connet(IPAddress.Parse(endPoint.ToString().Split(':')[0]), new Ports(int.Parse(endPoint.ToString().Split(':')[1])));// 实例化连接对象
            NewConnet.TimeOut += ConnetTimeOut;                                                    // 绑定登录超时方法
        }
        private void Messager(EndPoint endPoint, byte[] Context, int Length)
        {
            messageDis.AddMessage(endPoint, Encoding.UTF32.GetString(Context, 0, Length));         // 写入消息接收
        }
        private void ConnetError(EndPoint endPoint)
        {
            if (Login.ContainsKey(endPoint))
                Login.Remove(endPoint);
            if (Mmap.ContainsKey(endPoint))
                Mmap.Remove(endPoint);
            messageDis.DeleteMessage(endPoint);
        }
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
                if (Message[0] == "Login")
                {
                    string User = Message[1];
                    string Pwd = Message[2];
                    if (Select.SelectPassword(User) == Pwd)
                    {
                        SendMessage(endPoint, "Login OK 200");
                        Login.Add(endPoint, None[endPoint]);
                        Login[endPoint].ConnetType = ConnetType.Login;
                        None.Remove(endPoint);
                    }
                    else
                    {
                        SendMessage(endPoint, "Login Error 400");
                    }
                }
                if (Message[0] == "Mmap")
                {   // TCP,192.168.1.10，25565
                    string[] Mmap = Message[1].Split(',');
                    List<Mmaper> MmapList = new List<Mmaper>();
                    for (int i = 0; i < Mmap.Length; i = i + 1)
                    {
                        if (Select.SelectMmap(ThisConneter.User, Mmap[1], Mmap[2]))
                        {
                            Mmaper NewMmap = new Mmaper(ThisConneter.User, IPAddress.Parse(Mmap[1]), new Ports(int.Parse(Mmap[2])), Select.SelectMmapSpeed(ThisConneter.User, Mmap[1], Mmap[2]));
                            MmapList.Add(NewMmap);
                            Mmaps mmaps = new Mmaps(NewMmap, Select.SelectFlow(ThisConneter.User, Mmap[1], Mmap[2]));
                        }
                        else
                            SendMessage(endPoint, "Mmap Error " + Mmap[1] + ":" + Mmap[2] + "无映射权限");
                    }
                }
                if (Message[0] == "Select")
                {

                }
                if (Message[0] == "MmapInfo")
                {

                }
            }
        }
        private void SendMessage(EndPoint endPoint, string Context)
        {
            string Base64Context = Convert.ToBase64String(Encoding.UTF8.GetBytes(Context));        // 把要发送的字符串专为字节数组再转为base64代码
            byte[] SendMessage = Encoding.UTF8.GetBytes("<MappPackStart>{" + Base64Context + "}<MappPackEnd>");// 把base64字符串装入包内
            ServerTcp.Send(endPoint, SendMessage);                                                 // 发送消息
        }
        private void ConnetTimeOut(EndPoint endPoint)
        {
            SendMessage(endPoint, "Mppm Login TimeOut");                                           // 发送超时消息
            ServerTcp.Close(endPoint);                                                             // 关闭tcp连接
        }
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

    }
}