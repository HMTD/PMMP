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
        /// <summary>
        /// 修改剩余流量
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">IP</param>
        /// <param name="Port">端口</param>
        /// <param name="Flow">剩余流量</param>
        public void WriteSpend(string User, string IP, string Port, string Flow)
        {
            cc: if (LockSql_Mmap_db == true)
            {
                goto cc;
            }
            else
            {
                LockSql_Mmap_db = true;
                string FileContext = File.ReadAllText("Sql/Mmap");                                 // 获取映射权限储存文件
                string[] Context = FileContext.Split('\n');                                        // 按行分割
                int MmapInt = -1;                                                                  // 储存位置
                string MmapContextStr = null;
                for (int i = 0; i < Context.Length; i = i + 1)                                     // 遍历数组
                {
                    if (Context[i].Contains("Name=" + User + "&{"))                                // 找到对应用户名
                    {
                        MmapContextStr = Context[i].Substring(("Name=" + User + "&{").Length);     // 写入缓存
                        MmapInt = i;
                    }
                }
                if (MmapInt >= 0)
                {
                    List<string> MmapContext = new List<string>();
                    for (int i = 0; i < MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1; i = i + 1)// 遍历缓存
                    {
                        MmapContext.Add(MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[i]);// 写入集合
                    }
                    string LastMmap = MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1];
                    MmapContext.Add(LastMmap.Substring(0, LastMmap.IndexOf("}")));
                    for (int i = 0; i < MmapContext.Count; i = i + 1)
                    {
                        if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)// 核对映射
                        {
                            MmapContext[i] = MmapContext[i].Split(',')[0] + "," + MmapContext[i].Split(',')[1] + "," + MmapContext[i].Split(',')[2] + "," + MmapContext[i].Split(',')[3] + "," + Flow;
                        }
                    }
                    string Files = "Name=" + User + "&";
                    for (int i = 0; i < MmapContext.Count; i = i + 1)
                    {
                        Files = Files + "{" + MmapContext[i] + "}";
                    }
                    Context[MmapInt] = Files;
                    string FileWriteContext = Context[0].Replace("\r", "");
                    for (int i = 1; i < Context.Length; i = i + 1)
                        FileWriteContext = FileWriteContext + "\n" + Context[i].Replace("\r", "");
                    File.WriteAllText("Sql/Mmap.db", FileWriteContext);
                }
                LockSql_Mmap_db = false;
            }
        }
        /// <summary>
        /// 查询一个用户的所有映射权限的所有信息
        /// </summary>
        /// <param name="User"></param>
        /// <returns>Bug已消除</returns>
        public string SelectMmapAll(string User)
        {
            string FileContext = SelectContext("Mmap.db");                                         // 获取映射权限储存文件
            string[] Context = FileContext.Split('\n');                                            // 按行分割
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User + "&{"))                                    // 找到对应用户名
                    MmapContextStr = Context[i].Substring(("Name=" + User + "&{").Length - 1);     // 写入缓存
            }
            return MmapContextStr.Replace("\r", "");                                               // 返回数据
        }
        /// <summary>
        /// 查询密码
        /// </summary>
        /// <param name="User">用户名</param>
        /// <returns>Bug已消除</returns>
        public string SelectPassword(string User)
        {
            string FileContext = SelectContext("User.db");                                         // 获取用户名密码储存文件
            string[] Context = FileContext.Split('\n');                                            // 按行分割
            string Return = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Contains("Name=" + User + "&"))                                     // 找到用户名
                    Return = Context[i].Substring(("Name=" + User + "&Password=").Length).Replace("\r", "");// 返回密码
            }
            return Return;
        }
        /// <summary>
        /// 查询映射剩余流量
        /// </summary>
        /// <param name="User">用户名</param>
        /// <param name="IP">监听IP</param>
        /// <param name="Port">监听端口</param>
        /// <returns>Bug已排除</returns>
        public Flow SelectFlow(string User, string IP, string Port)
        {
            // Name=用户名{映射模式,本机IP,本机端口,映射速度Mbps,剩余流量MB}
            // Name=Admin{TCP,192.168.1.10,25565,1,∞}{TCP,192.168.1.10,25566,5,1024}
            string FileContext = SelectContext("Mmap.db");                                         // 获取映射权限储存文件
            string[] Context = FileContext.Split('\n');                                            // 按行分割
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数组
            {
                if (Context[i].Substring(5, Context[i].IndexOf("&") - 5) == User)                  // 找到对应用户名
                    MmapContextStr = Context[i].Substring(("Name=" + User + "&{").Length);         // 写入缓存
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1; i = i + 1)// 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[i]);// 找到对应映射
            }
            Flow Return = null;
            string LastMmap = MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1];
            MmapContext.Add(LastMmap.Substring(0, LastMmap.IndexOf("}")));
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
        /// <returns>Bug已消除</returns>
        public bool SelectMmap(string User, string IP, string Port)
        {
            string FileContext = SelectContext("Mmap.db");                                         // 获取映射规则文件内容
            string[] Context = FileContext.Split('\n');                                            // 分割数据
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数据
            {
                if (Context[i].Contains("Name=" + User + "&{"))
                    MmapContextStr = Context[i].Substring(("Name=" + User + "&{").Length);         // 找到对应数据
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1; i = i + 1)                  // 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[i]);                                    // 加入映射数组
            }
            bool Return = false;
            string LastMmap = MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1];
            MmapContext.Add(LastMmap.Substring(0, LastMmap.IndexOf("}")));
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
        /// <returns>Bug已消除</returns>
        public Speed SelectMmapSpeed(string User, string IP, string Port)
        {
            string FileContext = SelectContext("Mmap.db");                                         // 获取数据文件
            string[] Context = FileContext.Split('\n');                                            // 分割数据
            string MmapContextStr = null;
            for (int i = 0; i < Context.Length; i = i + 1)                                         // 遍历数据
            {
                if (Context[i].Contains("Name=" + User + "&{"))
                    MmapContextStr = Context[i].Substring(("Name=" + User + "&{").Length);         // 找到对应数据
            }
            List<string> MmapContext = new List<string>();
            for (int i = 0; i < MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1; i = i + 1)// 遍历缓存
            {
                MmapContext.Add(MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[i]);// 加入集合
            }
            Speed Return = null;
            string LastMmap = MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None)[MmapContextStr.Split(new string[] { "}{" }, StringSplitOptions.None).Length - 1];
            MmapContext.Add(LastMmap.Substring(0, LastMmap.IndexOf("}")));
            for (int i = 0; i < MmapContext.Count; i = i + 1)                                      // 遍历集合
            {
                if (MmapContext[i].Split(',')[1] == IP && MmapContext[i].Split(',')[2] == Port)    // 核对数据
                {
                    double length = double.Parse(MmapContext[i].Split(',')[3]) / (double)8 * 1024 * 1024;
                    Return = new Speed(length);                                                    // 返回数据
                }
            }
            return Return;
        }
        public Select()
        {
            LockSql_Mmap_db = false;
            LockSql_User_db = false;
            Sql_Mmap_dbContext = File.ReadAllText("Sql/Mmap.db", Encoding.UTF8);
            Sql_User_dbContext = File.ReadAllText("Sql/User.db", Encoding.UTF8);
        }
        private bool LockSql_Mmap_db { set; get; }
        private bool LockSql_User_db { set; get; }
        public string Sql_Mmap_dbContext { private set; get; }
        public string Sql_User_dbContext { private set; get; }
        public void WriteMmap_db(string Context)
        {
            cc: if (LockSql_Mmap_db == false)
            {
                LockSql_Mmap_db = true;
                Sql_Mmap_dbContext = Context;
                File.WriteAllText("Sql/Mmap.db", Context, Encoding.UTF8);
                LockSql_Mmap_db = false;
            }
            else
            {
                Thread.Sleep(5);
                goto cc;
            }
        }
        public void WriteUser_db(string Context)
        {
            cc: if (LockSql_Mmap_db == false)
            {
                LockSql_User_db = true;
                Sql_User_dbContext = Context;
                File.WriteAllText("Sql/User.db", Context, Encoding.UTF8);
                LockSql_User_db = false;
            }
            else
            {
                Thread.Sleep(5);
                goto cc;
            }
        }
        private string SelectContext(string FileName)
        {
            if (FileName == "Mmap.db")
            {
                if (LockSql_Mmap_db == true)
                {
                    Thread.Sleep(new TimeSpan(3000));
                    if (LockSql_Mmap_db == true)
                    {
                        cc: Thread.Sleep(5);
                        if (LockSql_Mmap_db == true) { goto cc; }
                        else { return Sql_Mmap_dbContext; }
                    }
                    else { return Sql_Mmap_dbContext; }
                }
                else { return Sql_Mmap_dbContext; }
            }
            else if (FileName == "User.db")
            {
                if (LockSql_User_db == true)
                {
                    Thread.Sleep(new TimeSpan(3000));
                    if (LockSql_User_db == true)
                    {
                        cc: Thread.Sleep(5);
                        if (LockSql_User_db == true) { goto cc; }
                        else { return Sql_User_dbContext; }
                    }
                    else { return Sql_User_dbContext; }
                }
                else { return Sql_User_dbContext; }
            }
            return null;
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
