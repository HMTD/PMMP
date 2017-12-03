using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PMMP
{
    class Select
    {
        /*
        Sql/User.db
        Sql/Mmap.db：
        Name=用户名{映射模式,本机IP,本机端口,映射速度,剩余流量}
        ........
        Name=Admin{TCP,192.168.1.10,25565,1,∞}
        ........
        */

        public static string SelectPassword(string User)
        {
            LogWriteLock.EnterWriteLock();
            string FileContext = File.ReadAllText("Sql/User.db");
            LogWriteLock.EnterWriteLock();
            string SqlContext = AES.DecryptByAES(FileContext, GetKey());
            string[] Context = SqlContext.Split('\r');
            string Return = null;
            for (int i = 0; i < Context.Length; i = i + 1)
            {
                if (Context[i].Contains("Name=" + User))
                    Return = Context[i].Substring(("Name=" + User).Length);
            }
            return Return;
        }
        public static Flow SelectFlow(string User, string IP, string Port)
        {
            LogWriteLock.EnterWriteLock();
            string FileContext = File.ReadAllText("Sql/Mmap.db");
            LogWriteLock.EnterWriteLock();
            string SqlContext = AES.DecryptByAES(FileContext, GetKey());
            string[] Context = SqlContext.Split('\r');
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)
            {
                if (Context[i].Contains("Name=" + User))
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);
            }
            Flow Return = null;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)
                {
                    Return = new Flow(double.Parse(MmapContext[i].Split(',')[4]));
                }
            }
            return Return;
        }
        public static bool SelectMmap(string User, string IP, string Port)
        {
            LogWriteLock.EnterWriteLock();
            string FileContext = File.ReadAllText("Sql/Mmap.db");
            LogWriteLock.EnterWriteLock();
            string SqlContext = AES.DecryptByAES(FileContext, GetKey());
            string[] Context = SqlContext.Split('\r');
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)
            {
                if (Context[i].Contains("Name=" + User))
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);
            }
            bool Return = false;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)
                {
                    Return = true;
                }
            }
            return Return;
        }
        public static Speed SelectMmapSpeed(string User, string IP, string Port)
        {
            LogWriteLock.EnterWriteLock();
            string FileContext = File.ReadAllText("Sql/Mmap.db");
            LogWriteLock.EnterWriteLock();
            string SqlContext = AES.DecryptByAES(FileContext, GetKey());
            string[] Context = SqlContext.Split('\r');
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)
            {
                if (Context[i].Contains("Name=" + User))
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);
            }
            Speed Return = null;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)
                {
                    Return = new Speed(int.Parse(MmapContext[i].Split(',')[3]) / 8 * 1024 * 1024);
                }
            }
            return Return;
        }
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        private static string GetKey()
        {
            string Return = null;
            LogWriteLock.EnterWriteLock();
            string[] FileContext = File.ReadAllLines("Config/PMMP.conf");
            LogWriteLock.EnterWriteLock();
            for (int i = 0; i < FileContext.Length; i = i + 1)
                if (FileContext[i].Contains(""))
                    Return = FileContext[i].Split('=')[1].Replace("\"", "");
            return Return;
        }

    }
    class AES
    {

        /// <summary>  
        /// AES加密算法  
        /// </summary>  
        /// <param name="input">明文字符串</param>  
        /// <param name="key">密钥</param>  
        /// <returns>字符串</returns>  
        public static string EncryptByAES(string input, string key)
        {
            byte[] AES_IV = Encoding.UTF8.GetBytes("IV");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32));
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = AES_IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                        byte[] bytes = msEncrypt.ToArray();
                        //return Convert.ToBase64String(bytes);//此方法不可用
                        return BitConverter.ToString(bytes);
                    }
                }
            }
        }
        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="input">密文字节数组</param>  
        /// <param name="key">密钥</param>  
        /// <returns>返回解密后的字符串</returns>  
        public static string DecryptByAES(string input, string key)
        {
            byte[] AES_IV = Encoding.UTF8.GetBytes("IV");
            //byte[] inputBytes = Convert.FromBase64String(input); //Encoding.UTF8.GetBytes(input);
            string[] sInput = input.Split("-".ToCharArray());
            byte[] inputBytes = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                inputBytes[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32));
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = AES_IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srEncrypt = new StreamReader(csEncrypt))
                        {
                            return srEncrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
