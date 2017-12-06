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
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        static ReaderWriterLockSlim LogWriteLock1 = new ReaderWriterLockSlim();
        /// <summary>
        /// 修改剩余流量
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">IP</param>
        /// <param name="Port">端口</param>
        /// <param name="Flow">剩余流量</param>
        public static void WriteSpend(string User, string IP, string Port, string Flow)
        {
            LogWriteLock1.EnterWriteLock();
            string FileContext = File.ReadAllText("Sql/Mmap.db");                                  // 获取映射权限储存文件
            string[] Context = FileContext.Split('\r');                                            // 按行分割
            int MmapInt = -1;                                                                      // 储存位置
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User))                                           // 找到对应用户名
                {
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length - 1);            // 写入缓存
                    MmapInt = i;
                }
            }
            if (MmapInt >= 0)
            {
                List<string> MmapContext = new List<string>();
                for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)                  // 遍历缓存
                {
                    MmapContext.Add(MmapContextStr.Split("}{")[i]);                                    // 写入集合
                }
                MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
                for (int i = 0; i < MmapContext.Count; i = i + 1)
                {
                    if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)    // 核对映射
                    {
                        MmapContext[i] = MmapContext[i].Split(',')[0] + "," + MmapContext[i].Split(',')[1] + "," + MmapContext[i].Split(',')[2] + "," + MmapContext[i].Split(',')[3] + "," + Flow;
                    }
                }
                string Files = "Name=" + User;
                for (int i = 0; i < MmapContext.Count; i = i + 1)
                {
                    Files = Files + "{" + MmapContext[i] + "}";
                }
                Context[MmapInt] = Files;
            }
        }
        /// <summary>
        /// 获取文件内容 带有排它锁
        /// </summary>
        /// <param name="FileRoad">文件路径</param>
        /// <returns></returns>
        public static string GetFile(string FileRoad)
        {
            LogWriteLock.EnterWriteLock();
            string FileContext = File.ReadAllText(FileRoad);
            LogWriteLock.EnterWriteLock();
            return FileContext;
        }
        /// <summary>
        /// 查询一个用户的所有映射权限的所有信息
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static string SelectMmapAll(string User)
        {
            string FileContext = GetFile("Sql/Mmap.db");                                           // 获取映射权限储存文件
            string[] Context = FileContext.Split('\r');                                            // 按行分割
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User))                                           // 找到对应用户名
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length - 1);            // 写入缓存
            }
            return MmapContextStr;                                                                 // 返回数据
        }
        /// <summary>
        /// 查询密码
        /// </summary>
        /// <param name="User">用户名</param>
        /// <returns></returns>
        public static string SelectPassword(string User)
        {
            string FileContext = GetFile("Sql/User.db");                                           // 获取用户名密码储存文件
            string[] Context = FileContext.Split('\r');                                            // 按行分割
            string Return = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User))                                           // 找到用户名
                    Return = Context[i].Substring(("Name=" + User).Length);                        // 返回密码
            }
            return Return;
        }
        /// <summary>
        /// 查询映射剩余流量
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">监听IP</param>
        /// <param name="Port">监听端口</param>
        /// <returns></returns>
        public static Flow SelectFlow(string User, string IP, string Port)
        {
            // Name=用户名{映射模式,本机IP,本机端口,映射速度Mbps,剩余流量MB}
            // Name=Admin{TCP,192.168.1.10,25565,1,∞}{TCP,192.168.1.10,25566,5,1024}
            string FileContext = GetFile("Sql/Mmap.db");                                           // 获取映射权限储存文件
            string[] Context = FileContext.Split('\r');                                            // 按行分割
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User))                                           // 找到对应用户名
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);                // 写入缓存
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)                  // 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);                                    // 找到对应映射
            }
            Flow Return = null;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)    // 获取映射速度
                {
                    if (MmapContext[i].Split(',')[4] == "∞")
                    {
                        Return = new Flow(0.0 / 0.0);
                        Return.IsInfinity = true;
                    }
                    else
                        Return = new Flow(double.Parse(MmapContext[i].Split(',')[4]));
                }
            }
            return Return;                                                                         // 返回
        }
        /// <summary>
        /// 查询映射
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">监听IP</param>
        /// <param name="Port">监听端口</param>
        /// <returns></returns>
        public static bool SelectMmap(string User, string IP, string Port)
        {
            string FileContext = GetFile("Sql/Mmap.db");                                           // 获取映射规则文件内容
            string[] Context = FileContext.Split('\r');                                            // 分割数据
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数据
            {
                if (Context[i].Contains("Name=" + User))
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);                // 找到对应数据
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)                  // 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);                                    // 加入映射数组
            }
            bool Return = false;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)    // 核对映射
                {
                    Return = true;                                                                 // 返回映射合法
                }
            }
            return Return;                                                                         // 返回
        }
        /// <summary>
        /// 查询映射速度
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">监听IP</param>
        /// <param name="Port">端口</param>
        /// <returns></returns>
        public static Speed SelectMmapSpeed(string User, string IP, string Port)
        {
            string FileContext = GetFile("Sql/Mmap.db");                                           // 获取数据文件
            string[] Context = FileContext.Split('\r');                                            // 分割数据
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数据
            {
                if (Context[i].Contains("Name=" + User))
                    MmapContextStr = Context[i].Substring(("Name=" + User).Length);                // 找到对应数据
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split("}{").Length - 1; i = i + 1)                  // 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split("}{")[i]);                                    // 加入集合
            }
            Speed Return = null;
            MmapContext.Add(MmapContextStr.Split("}{")[MmapContextStr.Split("}{").Length - 1].Replace("}", ""));
            for (int i = 0; i < MmapContext.Count; i = i + 1)                                      // 遍历集合
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)    // 核对数据
                {
                    Return = new Speed(int.Parse(MmapContext[i].Split(',')[3]) / 8 * 1024 * 1024); // 返回数据
                }
            }
            return Return;
        }

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
