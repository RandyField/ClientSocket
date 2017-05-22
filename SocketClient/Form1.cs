using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SocketClient
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }


        /// <summary>
        /// 窗体加载事件 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            InitSocketClient();
        }

        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region 连接


        /// <summary>
        /// 连接-开始对远程主机连接的异步请求
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress ip, int port)
        {
            //远程主机的 System.Net.IPAddress。
            //远程主机的端口号
            //requestCallback System.AsyncCallback 委托，它引用连接操作完成时要调用的方法
            //一个用户定义对象，其中包含连接操作的相关信息。操作完成时，此对象传递给了 requestCallback 委托。        
            this.clientSocket.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), this.clientSocket);
        }

        /// <summary>
        /// 连接操作完成时要调用的方法  可以连接为js中回调函数
        /// </summary>
        /// <param name="ar">异步操作的状态</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndConnect(ar);
            }
            catch (SocketException ex)
            { }
        }

        #endregion


        #region 发送


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            Send(System.Text.Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// 发送消息-byteData
        /// </summary>
        /// <param name="byteData"></param>
        private void Send(byte[] byteData)
        {
            try
            {
                //数据byte数组的长度
                int length = byteData.Length;

                this.clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this.clientSocket);
                ////头
                //byte[] head = BitConverter.GetBytes(length);

                ////待发送的数据
                //byte[] data = new byte[head.Length + byteData.Length];
                //Array.Copy(head, data, head.Length);
                //Array.Copy(byteData, 0, data, head.Length, byteData.Length);

                //发送
                //this.clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), this.clientSocket);
            }
            catch (SocketException ex)
            { }
        }

        /// <summary>
        /// 发送回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndSend(ar);
            }
            catch (SocketException ex)
            { }
        }

        #endregion


        #region 接收

        byte[] MsgBuffer = new byte[4096];

        /// <summary>
        /// 接收数据
        /// </summary>
        public void ReceiveData()
        {
            clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
        }

        /// <summary>
        /// 接收回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int REnd = clientSocket.EndReceive(ar);

                if (REnd > 0)
                {
                    byte[] data = new byte[REnd];
                    Array.Copy(MsgBuffer, 0, data, 0, REnd);

                    string str = System.Text.Encoding.UTF8.GetString(data);
                    txtRecord.Text += string.Format("{0}  服务端：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), str);
                    txtRecord.Text += "\r\n";
                    //在此次可以对data进行按需处理

                    clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                }
                else
                {
                    dispose();
                }
            }
            catch (SocketException ex)
            { }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        private void dispose()
        {
            try
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
                this.clientSocket.Close();
            }
            catch (Exception ex)
            { }
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitSocketClient()
        {
            int port = 8099;
            string host = "127.0.0.1";//服务器端ip地址

            IPAddress ip = IPAddress.Parse(host);
            //IPEndPoint ipe = new IPEndPoint(ip, port);
            Connect(ip, port);
            ReceiveData();
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            txtChat.Text = txtChat.Text.Trim();
            Send(txtChat.Text);
            txtRecord.Text += string.Format("{0}  客户端：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), txtChat.Text);
            txtRecord.Text += "\r\n";
            txtChat.Text = "";
        }

        private void txtChat_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\r")
            {
                txtChat.Text = txtChat.Text.Trim();
                //发送
                Send(txtChat.Text);
                txtRecord.Text += string.Format("{0}  客服端：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), txtChat.Text);
                txtRecord.Text += "\r\n";
                txtChat.Text = "";
                
               
            }
        }

        /// <summary>
        /// 客户端建立连接 发送消息 接收消息 关闭
        /// </summary>
        //public void InitSocketClient()
        //{
        //    try
        //    {
        //        int port = 8099;
        //        string host = "127.0.0.1";//服务器端ip地址

        //        IPAddress ip = IPAddress.Parse(host);
        //        IPEndPoint ipe = new IPEndPoint(ip, port);

        //        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        clientSocket.Connect(ipe);

        //        //send message
        //        string sendStr = "send to server : hello,ni hao";
        //        byte[] sendBytes = Encoding.ASCII.GetBytes(sendStr);
        //        clientSocket.Send(sendBytes);
        //        txtInfo.Text += string.Format("{0}:客服端发送信息{1}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sendStr);

        //        //receive message
        //        string recStr = "";
        //        byte[] recBytes = new byte[4096];
        //        int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
        //        recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
        //        txtInfo.Text += string.Format("{0}:收到服务端信息{1}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), recStr);

        //        clientSocket.Close();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

       
    }
}
