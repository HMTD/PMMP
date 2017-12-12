using System;
using System.Net;
using System.Net.Sockets;

namespace PMMP
{
    class Program
    {
        Control Ctrl;
        Select Select;
        static void Main(string[] args)
        {
            Program Program = new Program();
            Program.Ctrl = new Control(IPAddress.Parse("127.0.0.1"), new Ports(25565));
            Program.Ctrl.WriteSpend += Program.WriteSpend;
            Program.Ctrl.SelectFlow += Program.SelectFlow;
            Program.Ctrl.SelectMmap += Program.SelectMmap;
            Program.Ctrl.SelectMmapAll += Program.SelectMmapAll;
            Program.Ctrl.SelectMmapSpeed += Program.SelectMmapSpeed;
            Program.Ctrl.SelectPassword += Program.SelectPassword;
            Program.Select = new Select();
        }
        public void WriteSpend(string User, string IP, string Port, string Context)
        {
            Select.WriteSpend(User, IP, Port, Context);
        }
        public string SelectMmapAll(string User)
        {
            return Select.SelectMmapAll(User);
        }
        public string SelectPassword(string User)
        {
            return Select.SelectPassword(User);
        }
        public Flow SelectFlow(string User, string IP, string Port)
        {
            return Select.SelectFlow(User, IP, Port);
        }
        public bool SelectMmap(string User, string IP, string Port)
        {
            return Select.SelectMmap(User, IP, Port);
        }
        public Speed SelectMmapSpeed(string User, string IP, string Port)
        {
            return Select.SelectMmapSpeed(User, IP, Port);
        }
    }
}
