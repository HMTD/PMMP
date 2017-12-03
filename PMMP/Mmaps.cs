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
        public Mmaps(Mmaper mmaper, Flow flow)
        {
            IP = mmaper.LocalhostIP;
            Port = mmaper.LocalhostPort;
            speed = mmaper.MmapSpend;
            Flow = flow;
        }
        public IPAddress IP { set; get; }
        public Ports Port { set; get; }
        public Speed speed { set; get; }
        public Flow Flow { set; get; }
    }
}
