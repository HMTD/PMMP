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
        MessageDis messageDis;
        Dictionary<EndPoint, Connet> Login = new Dictionary<EndPoint, Connet>();
        Dictionary<EndPoint, Connet> Mmap = new Dictionary<EndPoint, Connet>();
        Dictionary<EndPoint, Connet> None = new Dictionary<EndPoint, Connet>();
        Dictionary<EndPoint, Mmaper> MmapDict = new Dictionary<EndPoint, Mmaper>();
        public delegate string[] SeleteMmapListDelegate(string User);
        public SeleteMmapListDelegate GetMmapList;
        public Control(IPAddress iPAddress, Ports Port)
        {
            ServerTcp = new Tcp(iPAddress, Port);
            ServerTcp.Conneter += Conneter;
            ServerTcp.ReceiveContext += Messager;
            ServerTcp.TcpError += Conneter;
            messageDis = new MessageDis();
            messageDis.CompletePack += MessagePack;
        }
        private void Conneter(EndPoint endPoint)
        {
            Connet NewConnet = new Connet(IPAddress.Parse(endPoint.ToString().Split(':')[0]), new Ports(int.Parse(endPoint.ToString().Split(':')[1])));
            NewConnet.TimeOut += ConnetTimeOut;
        }
        private void Messager(EndPoint endPoint, byte[] Context, int Length)
        {
            messageDis.AddMessage(endPoint, Encoding.UTF32.GetString(Context, 0, Length));
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
            for (int ic = 0; ic < Context.Length; ic = ic + 1)
            {
                string[] Message = Context[ic].Split(' ');
                if (Message[0] == "Login")
                {
                    string User = Message[1];
                    string Pwd = Message[2];
                    if (User == Pwd)
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
                {
                  
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
            string Base64Context = Convert.ToBase64String(Encoding.UTF8.GetBytes(Context));
            byte[] SendMessage = Encoding.UTF8.GetBytes("<MappPackStart>{" + Base64Context + "}<MappPackEnd>");
            ServerTcp.Send(endPoint, SendMessage);
            ServerTcp.Close(endPoint);
        }
        private void ConnetTimeOut(EndPoint endPoint)
        {
            string Base64Context = Convert.ToBase64String(Encoding.UTF8.GetBytes("Mppm Login TimeOut"));
            byte[] SendMessage = Encoding.UTF8.GetBytes("<MappPackStart>{" + Base64Context + "}<MappPackEnd>");
            ServerTcp.Send(endPoint, SendMessage);
            ServerTcp.Close(endPoint);
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