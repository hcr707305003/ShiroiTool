using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace ShiroiTool
{
    internal class SocketServer
    {
        public List<Socket> Sockets = new List<Socket>();

        public Socket listener;

        public int port = 8000; 

        public SocketServer(int p = 0)
        {
            if (port != 0)
                port = p;
        }

        public void listen()
        {
            byte[] buffer = new byte[1024];
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
            listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEP);
                listener.Listen(10);
                Console.WriteLine("等待客户端连接....");
                while (true) //该操作用于多个客户端连接
                {
                    Socket sc = listener.Accept();//接受一个连接
                    Debug.WriteLine(sc);
                    Sockets.Add(sc); //将连接的客户端, 添加到内存当中
                    Thread t = new Thread(new ThreadStart(() => ReceiveData(sc))); //开启当前Socket线程, 去执行获取数据的动作,与客户端通信
                    //t.IsBackground = true;
                    t.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();
        }

        public void unlisten()
        {
            foreach (Socket s in Sockets)
                s.Close();
            Sockets.Clear();
            listener.Close();
        }

        public void ReceiveData(Socket sc)
        {
            byte[] buffer = new byte[1024];
            Console.WriteLine("接受到了客户端：" + sc.RemoteEndPoint.ToString() + "连接....");
            //握手
            int length = sc.Receive(buffer);//接受客户端握手信息
            sc.Send(PackHandShakeData(GetSecKeyAccetp(buffer, length))); while (true)
            {
                try
                {
                    //接受客户端数据
                    Console.WriteLine("等待客户端数据....");
                    length = sc.Receive(buffer);//接受客户端信息
                    string clientMsg = AnalyticData(buffer, length);
                    Console.WriteLine("接受到客户端数据：" + clientMsg);
                    //发送数据
                    string sendMsg = "服务端返回信息:" + clientMsg;
                    sc.Send(PackData(sendMsg));
                }
                catch (Exception ex)
                {
                    Sockets.Remove(sc);  //如果接收的过程中,断开, 那么内存中移除当前Socket对象, 并且退出当前线程
                    Console.WriteLine("客户端已经断开连接!");
                    return;
                }
            }
        }

        /// <summary>
        /// 打包握手信息
        /// </summary>
        /// <param name="secKeyAccept"></param>
        /// <returns></returns>
        private byte[] PackHandShakeData(string secKeyAccept)
        {
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + Environment.NewLine);
            responseBuilder.Append("Upgrade: websocket" + Environment.NewLine);
            responseBuilder.Append("Connection: Upgrade" + Environment.NewLine);
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKeyAccept + Environment.NewLine + Environment.NewLine);
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 生成Sec-WebSocket-Accept
        /// </summary>
        /// <param name="handShakeText">客户端握手信息</param>
        /// <returns>Sec-WebSocket-Accept</returns>
        private string GetSecKeyAccetp(byte[] handShakeBytes, int bytesLength)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, bytesLength);
            string key = string.Empty;
            Regex r = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = r.Match(handShakeText);
            if (m.Groups.Count != 0)
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            byte[] encryptionString = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            return Convert.ToBase64String(encryptionString);
        }

        /// <summary>
        /// 解析客户端数据包
        /// </summary>
        /// <param name="recBytes">服务器接收的数据包</param>
        /// <param name="recByteLength">有效数据长度</param>
        /// <returns></returns>
        private string AnalyticData(byte[] recBytes, int recByteLength)
        {
            if (recByteLength < 2) { return string.Empty; }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return Encoding.UTF8.GetString(payload_data);
        }

        /// <summary>
        /// 打包服务器数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <returns>数据包</returns>
        private byte[] PackData(string message)
        {
            byte[] contentBytes = null;
            byte[] temp = Encoding.UTF8.GetBytes(message);

            if (temp.Length < 126)
            {
                contentBytes = new byte[temp.Length + 2];
                contentBytes[0] = 0x81;
                contentBytes[1] = (byte)temp.Length;
                Array.Copy(temp, 0, contentBytes, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                contentBytes = new byte[temp.Length + 4];
                contentBytes[0] = 0x81;
                contentBytes[1] = 126;
                contentBytes[2] = (byte)(temp.Length & 0xFF);
                contentBytes[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, contentBytes, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容  
            }

            return contentBytes;
        }
    }
}
