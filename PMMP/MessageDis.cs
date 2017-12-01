using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PMMP
{
    class MessageDis
    {
        Dictionary<EndPoint, string> MessageDict = new Dictionary<EndPoint, string>();
        public delegate void CompletePackDelegate(EndPoint endPoint, string[] Context);
        public CompletePackDelegate CompletePack;
        public void AddMessage(EndPoint endPoint, string Context)
        {
            if (MessageDict.ContainsKey(endPoint))
            {
                MessageDict[endPoint] = MessageDict[endPoint] + Context;
                string[] Pack = Dispose(MessageDict[endPoint]);
                if (Pack != null)
                {
                    List<string> PackList = new List<string>();
                    for (int i = 0; i < Pack.Length - 1; i = i + 1)
                        PackList.Add(Pack[i]);
                    CompletePack(endPoint, PackList.ToArray());
                }
            }
            else
            {
                MessageDict.Add(endPoint, Context);
            }
        }
        public void DeleteMessage(EndPoint endPoint)
        {
            MessageDict.Remove(endPoint);
        }
        private string[] Dispose(string Context)
        {
            if (Context.Contains("<PmmpPackStart>") && Context.Contains("<PmmpPackEnd>"))
            {
                List<string> Pack = new List<string>();
                for (int i = 0; i < Context.Split("<PmmpPackEnd>").Length - 1; i = i + 1)
                {
                    Pack.Add(Encoding.UTF8.GetString(Convert.FromBase64String(Context.Split("<PmmpPackEnd>")[i].Replace("<PmmpPackStart>", ""))));
                }
                Pack.Add(Context.Split("<PmmpPackEnd>")[Context.Split("<PmmpPackEnd>").Length - 1]);
                return Pack.ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
